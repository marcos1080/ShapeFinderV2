using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    public class PixelCollection
    {
        public PixelCollection(int x, int y)
        {
            Top = y;
            Rows = new Dictionary<int, SortedList<int, int>>();
            Left = x;
            Right = x;
            Count = 0;
        }

        // Each Y row contains the X pixel locations.
        public Dictionary<int, SortedList<int, int>> Rows { get; private set; }

        // Initial Y co-ordinate.
        public int Top { get; }

        // Lower bounds of shape.
        public int Bottom { get { return Top + Rows.Count - 1; } }

        // Left bounds of shape.
        public int Left { get; private set; }

        // Right bounds of shape.
        public int Right { get; private set; }

        // Number of pixels in the shape.
        public int Count { get; private set; }

        /// <summary>
        /// Add a new pixel to the shape.
        /// </summary>
        /// <param name="p"></param>
        public void AddPixel(Point p)
        {
            AddPixel(p.X, p.Y);
        }

        /// <summary>
        /// Add a new pixel to the shape.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPixel(int x, int y)
        {
            // New line
            if (!Rows.ContainsKey(y))
            {
                Rows.Add(y, new SortedList<int, int>());
            }

            // Add x co-ord to y line.
            if (!Rows[y].ContainsKey(x))
            {
                Rows[y].Add(x, x);
            }

            // Adjust left bounds.
            if (x < Left)
            {
                Left = x;
            }

            // Adjust right bounds.
            if (x > Right)
            {
                Right = x;
            }

            Count++;
        }

        /// <summary>
        /// Check if the pixel collection has a n existing Y row.
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool HasRow(int y)
        {
            if (Rows.ContainsKey(y))
            {
                return true;
            }

            return false;
        }
    }
}
