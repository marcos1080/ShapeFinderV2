using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Class that scans an image and creates groups of pixel co-ordinates
    /// for each shape found.
    /// Uses a linked list to preserve the order of shapes while scanning
    /// a line. This allows for shape groupings to be added and removed
    /// easily.
    /// </summary>
    public class ImageScanner
    {
        private Bitmap _image;
        private LinkedList<ShapeData> _currentShapes;
        private LinkedListNode<ShapeData> _current;
        private LinkedListNode<ShapeData> _expected;
        private Color _initialColour;

        /// <summary>
        /// Collection of the data for each shape.
        /// </summary>
        public List<ShapeData> Shapes { get; }

        public ImageScanner(Bitmap image)
        {
            _image = image;
            _currentShapes = new LinkedList<ShapeData>();
            _initialColour = _image.GetPixel(0, 0);
            Shapes = new List<ShapeData>();
        }

        /// <summary>
        /// Scan the image for shapes.
        /// </summary>
        public void Scan()
        {
            // Initialise search state.
            var activeShapes = new List<int>();
            

            for (int y = 0; y < _image.Height; y++)
            {
                // Current will not be null if the current pixels are within a shape.
                _current = null;

                // Expected is a linked list node of the shape that is expected to 
                // be encountered.
                _expected = _currentShapes.First;

                for (int x = 0; x < _image.Width; x++)
                {
                    // Perform tests on the pixel.
                    ProcessPixel(x, y);
                }

                // If expected not null then this indicates that it was not hit on this pass.
                // Can finalise it and remove it from the current shape list.
                if (_expected != null)
                {
                    Shapes.Add(_expected.Value);
                    _currentShapes.Remove(_expected);
                }
            }
        }

        /// <summary>
        /// Perform checks on a pixel to see if it is in a shape.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void ProcessPixel(int x, int y)
        {
            var colour = _image.GetPixel(x, y);

            // Not in a shape.
            if (colour == _initialColour)
            {
                // Indicates the last pixel was in a shape.
                if (_current != null)
                {
                    // Move the expected node down the list.
                    _expected = _current.Next;

                    // Unsetting here means next time a shape pixel is detected
                    // action can be taken to determine what shape it belongs to.
                    _current = null;
                }

                return;
            }

            // Entered a new shape.
            if (_current == null)
            {
                // Create or find current shape.
                if (_expected == null)
                {
                    // Check if connected to the last one in the list.
                    if (_currentShapes.Count > 0 && IsInShape(_currentShapes.Last.Value, x, y))
                    {
                        _current = _currentShapes.Last;
                    }
                    else
                    {
                        // There are either no shapes currently in the working list or this is
                        // a new shape detected after the last one in the list. Add new shape.
                        _current = _currentShapes.AddLast(new ShapeData(x, y));
                    }
                }
                else
                {
                    // Need to determine which shape this is.
                    _current = FindCurrent(x, y);
                }
            }

            // Add pixel co-ordinates to the current working shape.
            _current.Value.AddPixel(x, y);
        }

        /// <summary>
        /// Find the shape that this pixel belongs to.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private LinkedListNode<ShapeData> FindCurrent(int x, int y)
        {
            // While not at the end of the list and not in the shape that is expected next.
            while (_expected != null && !IsInShape(_expected.Value, x, y))
            {
                var next = _expected.Next;

                // Check which side of expecting shape this pixel is in.
                if (x > _expected.Value.LineEnd.X)
                {
                    // Pixel is positioned to the right of the last shape.
                    // This means that the expected shape has not been hit.
                    // Move from working list to finalised shape list.
                    Shapes.Add(_expected.Value);
                    _currentShapes.Remove(_expected);
                }
                else
                {
                    // Pixel is to the left of the expected shape.
                    // Insert a new shape into the list before the expected one.
                    return _currentShapes.AddBefore(_expected, new ShapeData(x, y));
                }

                // If this point is reached, then the expected shape was not
                // encountered and therefore removed. Now need to check if there
                // was a shape in the list after it.
                if (next == null)
                {
                    // There is no shape after, add a new one to the end.
                    return _currentShapes.AddLast(new ShapeData(x, y));
                } 
                else if (!IsInShape(next.Value, x, y))
                {
                    // Shape detected after. 
                    // Pixel is in that shape. Insert a new one in front of it.
                    return _currentShapes.AddBefore(next, new ShapeData(x, y));
                }

                // Pixel does not belong to the expected shape or the one after it.
                // Move down the list to check on next pass.
                _expected = next;
            }
            
            // Pixel is in the expected shape.
            return _expected;
        }

        /// <summary>
        /// Simple test to see if a shape is connected to this pixel.
        /// Not an exact test. Checks if there is a continuous line between this
        /// pixel and the next. Checks this line, the one above and the one below.
        /// If none of these lines connect the pixels they are not connected.
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsInShape(ShapeData shape, int x, int y)
        {
            Point lineStart = shape.LineStart;
            
            // Current pixel is to the left the start of the last scanned line of the shape.
            if (x < lineStart.X)
            {
                // Test if a line connects this pixel and the start of the last line.
                for (int i = x; i <= lineStart.X; i++)
                {
                    if (_image.GetPixel(i, y + 1) == _initialColour &&
                        _image.GetPixel(i, y - 1) == _initialColour &&
                        _image.GetPixel(i, y) == _initialColour)
                    {
                        // Break detected.
                        return false;
                    }
                }
            }

            Point lineEnd = shape.LineEnd;

            // Current pixel is to the right of the end of the last scanned line of the shape.
            if (x > lineEnd.X) {
                // Test if a line connects this pixel and the end of the last line.
                for (int i = lineEnd.X; i <= x; i++)
                {
                    if (_image.GetPixel(i, y + 1) == _initialColour &&
                        _image.GetPixel(i, y - 1) == _initialColour &&
                        _image.GetPixel(i, y) == _initialColour)
                    {
                        // Break detected.
                        return false;
                    }
                }
            }

            // They are connected.
            return true;
        }
    }
}
