using ShapeFinderV2.Helpers;
using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Class that scans an image and creates groups of pixel co-ordinates
    /// for each shape found.
    /// </summary>
    public class ImageScanner
    {
        private Bitmap _image;
        private PixelMap _pixelMap;

        /// <summary>
        /// Collection of the data for each shape.
        /// </summary>
        public List<ShapeData> Shapes { get; }

        public ImageScanner(Bitmap image, int colorThreshold)
        {
            _image = image;
            _pixelMap = new PixelMap(image, colorThreshold);
            Shapes = new List<ShapeData>();
        }

        /// <summary>
        /// Scan the image for shapes.
        /// </summary>
        public void Scan()
        {
            for (int y = 0; y < _image.Height; y++)
            {

                for (int x = 0; x < _image.Width; x++)
                {
                    if (_pixelMap.Visited(x, y))
                    {
                        // Pixel has already been visited, skip.
                        continue;
                    }

                    if (!_pixelMap.PixelInShape(x, y))
                    {
                        // Pixel is not in a shape.
                        _pixelMap.SetVisited(x, y);
                    }
                    else
                    {
                        // New shape found.
                        ShapeScanner scanner = new ShapeScanner(_image, _pixelMap);
                        Shapes.Add(scanner.Scan(x, y));
                    }
                }
            }
        }
    }
}
