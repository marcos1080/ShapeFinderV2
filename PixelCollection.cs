using System.Collections.Generic;

namespace ShapeFinderV2
{
    public class PixelCollection
    {
        public PixelCollection(int x, int y)
        {
            Top = y;
            Rows = new Dictionary<int, List<int>>();
            Left = x;
            Right = x;
            Count = 0;
        }

        public Dictionary<int, List<int>> Rows { get; private set; }

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
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPixel(int x, int y)
        {
            // New line
            if (!Rows.ContainsKey(y))
            {
                Rows.Add(y, new List<int>());

                // Adjust left bounds.
                if (x < Left)
                {
                    Left = x;
                }
            }

            // Add x co-ord to y line.
            Rows[y].Add(x);

            // Adjust right bounds.
            if (x > Right)
            {
                Right = x;
            }

            Count++;
        }

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
