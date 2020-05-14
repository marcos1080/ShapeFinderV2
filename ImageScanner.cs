using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ShapeFinderV2
{
    public class ImageScanner
    {
        private Bitmap _image;
        private LinkedList<ShapeData> _currentShapes;
        private LinkedListNode<ShapeData> _current;
        private LinkedListNode<ShapeData> _expected;
        private Color _initialColour;

        public List<ShapeData> Shapes { get; }

        public ImageScanner(Bitmap image)
        {
            _image = image;
            _currentShapes = new LinkedList<ShapeData>();
            _initialColour = _image.GetPixel(0, 0);
            Shapes = new List<ShapeData>();
        }

        public void Scan()
        {
            // Initialise search state.
            var activeShapes = new List<int>();
            

            for (int y = 0; y < _image.Height; y++)
            {
                _current = null;
                _expected = _currentShapes.First;

                for (int x = 0; x < _image.Width; x++)
                {
                    ProcessPixel(x, y);
                }

                // If expected not null then can finalise it as it wasn't hit on this pass.
                if (_expected != null)
                {
                    _expected.Value.Finalise();
                    Shapes.Add(_expected.Value);
                    _currentShapes.Remove(_expected);
                }
            }
        }

        private void ProcessPixel(int x, int y)
        {
            var colour = _image.GetPixel(x, y);

            if (colour == _initialColour)
            {
                if (_current != null)
                {
                    //Console.WriteLine($"Exiting Line: x = {x}, y = {y}");
                    _expected = _current.Next;
                    _current = null;
                }

                return;
            }

            if (_current == null)
            {
                //Console.WriteLine($"Entering Line: x = {x}, y = {y}");
                // Create or find current shape.
                if (_expected == null)
                {
                    // Check if connected to the last one.
                    if (_currentShapes.Count > 0 && IsInShape(_currentShapes.Last.Value, x, y))
                    {
                        _current = _currentShapes.Last;
                    }
                    else
                    {
                        _current = _currentShapes.AddLast(new ShapeData(x, y));
                    }
                }
                else
                {
                    // Need to determine which shape this is.
                    _current = FindCurrent(x, y);
                }
            }

            _current.Value.AddPixel(x, y);
        }

        private LinkedListNode<ShapeData> FindCurrent(int x, int y)
        {
            while (_expected != null && !IsInShape(_expected.Value, x, y))
            {
                var next = _expected.Next;

                // Check which side of expected this pixel is in.
                if (x > _expected.Value.LineEnd.X)
                {
                    // Remove expected from list as it is finished.
                    _expected.Value.Finalise();
                    Shapes.Add(_expected.Value);
                    _currentShapes.Remove(_expected);
                }
                else
                {
                    // New shape before expected.
                    return _currentShapes.AddBefore(_expected, new ShapeData(x, y));
                }

                // Check if need to create a new shape.
                if (next == null)
                {
                    return _currentShapes.AddLast(new ShapeData(x, y));
                } 
                else if (!IsInShape(next.Value, x, y))
                {
                    return _currentShapes.AddBefore(next, new ShapeData(x, y));
                }

                _expected = next;
            }

            return _expected;
        }

        private bool IsInShape(ShapeData shape, int x, int y)
        {
            Point lineStart = shape.LineStart;

            if (x < lineStart.X)
            {
                // Test if a line connects this pixel and the start of the last line.
                for (int i = x; i <= lineStart.X; i++)
                {
                    if (_image.GetPixel(i, y + 1) == _initialColour &&
                        _image.GetPixel(i, y - 1) == _initialColour &&
                        _image.GetPixel(i, y) == _initialColour)
                    {
                        return false;
                    }
                }
            }

            Point lineEnd = shape.LineEnd;

            if (x > lineEnd.X) {
                // Test if a line connects this pixel and the end of the last line.
                for (int i = lineEnd.X; i <= x; i++)
                {
                    if (_image.GetPixel(i, y + 1) == _initialColour &&
                        _image.GetPixel(i, y - 1) == _initialColour &&
                        _image.GetPixel(i, y) == _initialColour)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
