using System;
using System.Drawing;

namespace ShapeFinderV2.Helpers
{
    /// <summary>
    /// Hold and check data for each pixel in an image.
    /// </summary>
    public class PixelMap
    {
        private int _threshold;
        private int _initialColour;
        private Bitmap _image;
        private bool[][] _visited;

        public PixelMap(Bitmap image, int threshold)
        {
            _image = image;
            _threshold = threshold;
            _initialColour = ConvertToGrey(image.GetPixel(0, 0));

            // Initialise visited jagged array.
            _visited = new bool[image.Height][];
            for (int r = 0; r < image.Height; r++)
            {
                _visited[r] = new bool[image.Width];
            }
        }
        /// <summary>
        /// Simple check to see if the pixel is a different colour to the background..
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool PixelInShape(int x, int y)
        {
            // Convert pixel colour to greyscale value.
            Color colour = _image.GetPixel(x, y);
            int grey = ConvertToGrey(colour);

            // If the colour differs from the background more than the threshold
            // pixel colour is white, otherwise black.
            return Math.Abs(_initialColour - grey) > _threshold ? true : false;
        }

        /// <summary>
        /// Check if a pixel has already been visited.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Visited(int x, int y)
        {
            if (_visited[y][x])
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Toggle pixels visited state to true;
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetVisited(int x , int y)
        {
            _visited[y][x] = true;
        }

        /// <summary>
        /// Convert colour of pixel to a greyscale integer value.
        /// </summary>
        /// <param name="colour"></param>
        /// <returns></returns>
        private static int ConvertToGrey(Color colour)
        {
            return Convert.ToInt32(.21 * colour.R + .71 * colour.G + .071 * colour.B);
        }
    }
}
