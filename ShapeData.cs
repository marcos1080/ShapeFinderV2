using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Text;

namespace ShapeFinderV2
{
    /// <summary>
    /// Class that holds the raw scan data for a shape.
    /// </summary>
    public class ShapeData
    {
        // Areas around current pixel.
        private enum Zone
        {
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left
        }

        // Initial x co-ordinate.
        private readonly int _initX;

        // Sort of like an indexed jagged array of pixel co-ordinates.
        public Dictionary<int, List<int>> Pixels { get; private set; }

        // Start of the last scanned line of shape.
        public Point LineStart { get; private set; }

        // End of the last scanned line of shape.
        public Point LineEnd { get; private set; }

        // Sequential co-ordinates of the edge of the shape in a clock-wise direction.
        public List<Point> Contour { get; private set; }

        public ShapeData(int x, int y)
        {
            _initX = x;
            Top = y;
            Pixels = new Dictionary<int, List<int>>();
            Left = x;
            Right = x;
            Area = 0;
            Contour = new List<Point>();
        }

        // Initial Y co-ordinate.
        public int Top { get; }

        // Lower bounds of shape.
        public int Bottom { get { return Top + Pixels.Count - 1; } }

        // Left bounds of shape.
        public int Left { get; private set; }

        // Right bounds of shape.
        public int Right { get; private set; }

        // Number of pixels in the shape.
        public int Area { get; private set; }

        /// <summary>
        /// Add a new pixel to the shape.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPixel(int x, int y)
        {
            // New line
            if (!Pixels.ContainsKey(y))
            {
                Pixels.Add(y, new List<int>());
                LineStart = new Point(x, y);

                // Adjust left bounds.
                if (x < Left)
                {
                    Left = x;
                }
            }

            // Add x co-ord to y line.
            Pixels[y].Add(x);
            LineEnd = new Point(x, y);

            // Adjust right bounds.
            if (x > Right)
            {
                Right = x;
            }
            Area++;
        }

        /// <summary>
        /// Crawl around the edge of the shape to define it's contour.
        /// </summary>
        public void Finalise()
        {
            // Define contour.

            // Start from the end of the top line.
            var initial = new Point(Pixels[Top][^1], Top);
            Contour.Add(initial);

            // Used to hold the previous pixel co-ords.
            var last = initial;

            // Initialise current to be the next pixel along the edge.

            // Next pixel will be on the next line.
            int y = last.Y + 1; 
            // If line does not have a pixel to the bottom right then it must be below
            // initial pixel.
            int x = Pixels[y].Contains(last.X + 1) ? last.X + 1 : last.X;
            
            var current = new Point(x, y);

            // Crawl around contour until back at start.
            while (current != initial)
            {
                Contour.Add(current);

                // Find the position of the first empty pixel next to the previous one
                // in a clockwise direction.
                Zone initialSearchZone = FindSearchZone(current, last);

                // Search in a clockwise direction until the first shape pixel is found.
                Point found = FindNextPixel(initialSearchZone, current);

                // Increment
                last = current;
                current = found;
            }
        }

        /// <summary>
        /// Basic method of finding the first blank pixel in a clockwise
        /// direction from the previous pixel.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        private Zone FindSearchZone(Point current, Point last)
        {
            // Previous pixel to the left of the current one.
            if (last.X < current.X)
            {
                // Previous above current.
                if (last.Y < current.Y)
                {
                    return Zone.Top;
                }
                // Previous below current.
                if (last.Y > current.Y)
                {
                    return Zone.Left;
                }
                // Previous beside current.
                return Zone.TopLeft;
            }

            // Previous pixel to the right of the current one.
            if (last.X > current.X)
            {
                // Previous above current.
                if (last.Y < current.Y)
                {
                    return Zone.Right;
                }
                // Previous below current.
                if (last.Y > current.Y)
                {
                    return Zone.Bottom;
                }
                // Previous beside current.
                return Zone.BottomRight;
            }

            // Previous pixel to the above or below of the current one.

            // Previous pixel below current.
            if (last.Y > current.Y)
            {
                return Zone.BottomLeft;
            }

            // Previous pixel above current.
            return Zone.TopRight;
        }

        /// <summary>
        /// Iterate in a clockwise direction around the current pixel from the
        /// initial search zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="pixel"></param>
        /// <returns></returns>
        private Point FindNextPixel(Zone zone, Point pixel)
        {
            Zone endZone = zone;

            // Determine the end zone.
            // Zone directly adjacent to the initial in an anti clockwise direction.
            if (endZone == Zone.TopLeft)
            {
                endZone = Zone.Left;
            }
            else
            {
                endZone--;
            }

            while (zone != endZone)
            {
                if (zone == Zone.TopLeft && Pixels.ContainsKey(pixel.Y - 1) 
                    && Pixels[pixel.Y - 1].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y - 1);
                }
                else if (zone == Zone.Top && Pixels.ContainsKey(pixel.Y - 1) 
                    && Pixels[pixel.Y - 1].Contains(pixel.X))
                {
                    return new Point(pixel.X, pixel.Y - 1);
                }
                else if (zone == Zone.TopRight && Pixels.ContainsKey(pixel.Y - 1) 
                    && Pixels[pixel.Y - 1].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y - 1);
                }
                else if (zone == Zone.Right && Pixels[pixel.Y].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y);
                }
                else if (zone == Zone.BottomRight && Pixels.ContainsKey(pixel.Y + 1) 
                    && Pixels[pixel.Y + 1].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y + 1);
                }
                else if (zone == Zone.Bottom && Pixels.ContainsKey(pixel.Y + 1) 
                    && Pixels[pixel.Y + 1].Contains(pixel.X))
                {
                    return new Point(pixel.X, pixel.Y + 1);
                }
                else if (zone == Zone.BottomLeft && Pixels.ContainsKey(pixel.Y + 1) 
                    && Pixels[pixel.Y + 1].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y + 1);
                }
                else if (zone == Zone.Left && Pixels[pixel.Y].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y);
                }

                // Iterate.
                if (zone == Zone.Left)
                {
                    zone = Zone.TopLeft;
                } else
                {
                    zone++;
                }
            }

            // Never reached.
            return new Point(0, 0);
        }
    }
}
