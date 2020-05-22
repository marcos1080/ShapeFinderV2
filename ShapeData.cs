using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Class that holds the raw scan data for a shape.
    /// </summary>
    public class ShapeData
    {
        // Sort of like an indexed jagged array of pixel co-ordinates.
        public PixelCollection Pixels { get; }

        // Start of the last scanned line of shape.
        public Point LineStart { get; private set; }

        // End of the last scanned line of shape.
        public Point LineEnd { get; private set; }

        public ShapeData(int x, int y)
        {
            Pixels = new PixelCollection(x, y);
        }

        /// <summary>
        /// Add a new pixel to the shape.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPixel(int x, int y)
        {
            // New line
            if (!Pixels.HasRow(y))
            {
                LineStart = new Point(x, y);
            }

            // Add x co-ord to y line.
            LineEnd = new Point(x, y);

            Pixels.AddPixel(x, y);
        }
    }
}
