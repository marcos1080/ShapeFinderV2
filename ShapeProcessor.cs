using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2
{
    public class ShapeProcessor
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

        private enum ShapeType
        {
            Circle,
            Triangle,
            Quadralateral,
            Unknown,
            Invalid
        }

        // Angle threshold that indicates a corner.
        private readonly double _threshold;

        public ShapeProcessor(double threshold) 
        {
            _threshold = threshold;
        }

        public Shape InferShape(PixelCollection pixels)
        {
            List<Point> contour = DefineContour(pixels);

            // Process Contour.
            List<Line> lines = FindLines(contour);
            List<Corner> corners = FindCorners(lines);
            ShapeType type = InferShape(lines, corners, pixels);

            switch (type)
            {
                case ShapeType.Circle:
                    return new Circle(contour, pixels, lines, corners);
                case ShapeType.Triangle:
                    return new Triangle(contour, pixels, lines, corners);
                case ShapeType.Quadralateral:
                    return new Quadralateral(contour, pixels, lines, corners);
                default:
                    throw new Exception();
            }
        }

        private List<Point> DefineContour(PixelCollection pixels)
        {
            // Define contour.
            var contour = new List<Point>();

            // Start from the end of the top line.
            var initial = new Point(pixels.Rows[pixels.Top][^1], pixels.Top);
            contour.Add(initial);

            // Used to hold the previous pixel co-ords.
            var last = initial;

            // Initialise current to be the next pixel along the edge.

            // Next pixel will be on the next line.
            int y = last.Y + 1;
            // If line does not have a pixel to the bottom right then it must be below
            // initial pixel.
            int x = pixels.Rows[y].Contains(last.X + 1) ? last.X + 1 : last.X;

            var current = new Point(x, y);

            // Crawl around contour until back at start.
            while (current != initial)
            {
                contour.Add(current);

                // Find the position of the first empty pixel next to the previous one
                // in a clockwise direction.
                Zone initialSearchZone = FindSearchZone(current, last);

                // Search in a clockwise direction until the first shape pixel is found.
                Point found = FindNextPixel(initialSearchZone, current, pixels.Rows);

                // Increment
                last = current;
                current = found;
            }

            return contour;
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
        private Point FindNextPixel(Zone zone, Point pixel, Dictionary<int, List<int>> rows)
        {
            while (true)
            {
                if (zone == Zone.TopLeft && rows.ContainsKey(pixel.Y - 1)
                    && rows[pixel.Y - 1].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y - 1);
                }
                else if (zone == Zone.Top && rows.ContainsKey(pixel.Y - 1)
                    && rows[pixel.Y - 1].Contains(pixel.X))
                {
                    return new Point(pixel.X, pixel.Y - 1);
                }
                else if (zone == Zone.TopRight && rows.ContainsKey(pixel.Y - 1)
                    && rows[pixel.Y - 1].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y - 1);
                }
                else if (zone == Zone.Right && rows[pixel.Y].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y);
                }
                else if (zone == Zone.BottomRight && rows.ContainsKey(pixel.Y + 1)
                    && rows[pixel.Y + 1].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y + 1);
                }
                else if (zone == Zone.Bottom && rows.ContainsKey(pixel.Y + 1)
                    && rows[pixel.Y + 1].Contains(pixel.X))
                {
                    return new Point(pixel.X, pixel.Y + 1);
                }
                else if (zone == Zone.BottomLeft && rows.ContainsKey(pixel.Y + 1)
                    && rows[pixel.Y + 1].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y + 1);
                }
                else if (zone == Zone.Left && rows[pixel.Y].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y);
                }

                // Iterate.
                if (zone == Zone.Left)
                {
                    zone = Zone.TopLeft;
                }
                else
                {
                    zone++;
                }
            }
        }

        /// <summary>
        /// Crawl around contour and detect straight lines from corners.
        /// </summary>
        /// <returns></returns>
        private List<Line> FindLines(List<Point> contour)
        {
            // Init boundaries.
            //Top = Contour[0].Y;
            //Bottom = Contour[0].Y;
            //Left = Contour[0].X;
            //Right = Contour[0].X;

            // Sampling interval.
            int interval = Math.Max(3, Convert.ToInt32(contour.Count * 0.02));

            var lines = new List<Line>();
            var currentLine = new Line(contour[1]);
            var crawler = new ContourCrawler(contour[0]);
            int distance = interval;

            while (distance < contour.Count)
            {
                Point currentPoint = contour[distance];

                // Adjust the bounds.
                // Using this method as a circle may only yield 1 line.
                //Right = Math.Max(Right, currentPoint.X);
                //Left = Math.Min(Left, currentPoint.X);
                //Bottom = Math.Max(Bottom, currentPoint.Y);

                crawler.Add(currentPoint);

                // Line is changing angle enough to be a corner.
                if (Math.Abs(crawler.Deviation) > _threshold)
                {
                    // Angle indicates a new line.
                    lines.Add(currentLine);

                    // Remove last point to avoid skewing the results.
                    currentLine.RemoveLast();

                    // Start next line.
                    currentLine = new Line(currentPoint);

                    // Reset the crawler
                    crawler = new ContourCrawler(contour[distance]);
                }
                else
                {
                    currentLine.Add(currentPoint);
                }

                distance += interval;
            }

            // Move last line to list.
            if (currentLine.Start != currentLine.End)
            {
                lines.Add(currentLine);
            }

            // Add last point to  line if it's a likely circle.
            // Used to get proper length of perimeter.
            if (lines.Count == 1 && currentLine.End != contour[^1])
            {
                currentLine.Add(contour[^1]);
            }

            return lines;
        }

        /// <summary>
        /// Determine the co-ordinates of the corners using the intersection of
        /// adjacent lines.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private List<Corner> FindCorners(List<Line> lines)
        {
            var corners = new List<Corner>();

            // Indicates a circle.
            if (lines.Count == 1)
            {
                return corners;
            }

            // Indicates a line.
            if (lines.Count == 2)
            {
                throw new Exception("Line");
            }

            Line a, b;
            Point corner;

            // Find corner co-ordinate by finding intersection of lines.
            for (int i = 1; i < lines.Count; i++)
            {
                a = lines[i - 1];
                b = lines[i];
                corner = Geometry.FindIntersection(a, b);

                corners.Add(new Corner()
                {
                    Coordinate = corner,
                    Angle = Geometry.AngleDifference(a, b)
                });
            }

            // Compare first and last lines to get final corner.
            a = lines[^1];
            b = lines[0];
            corner = Geometry.FindIntersection(a, b);

            corners.Add(new Corner()
            {
                Coordinate = corner,
                Angle = Geometry.AngleDifference(a, b)
            });

            // Add corner co-ordinates to the corresponding lines, if not
            // present already.
            for (int i = 0; i < corners.Count; i++)
            {
                a = lines[i];
                b = i == corners.Count - 1 ? lines[0] : lines[i + 1];

                a.AdjustEnd(corners[i].Coordinate);
                b.AdjustStart(corners[i].Coordinate);
            }

            return corners;
        }

        /// <summary>
        /// Use data points to infer what shape it is.
        /// </summary>
        private ShapeType InferShape(List<Line> lines, List<Corner> corners, PixelCollection pixels)
        {
            if (lines.Count == 1 || corners.Count > 4)
            {
                return ShapeType.Circle;
            }

            if (corners.Count == 3)
            {
                return ShapeType.Triangle;
            }

            if (corners.Count == 4)
            {
                return ShapeType.Quadralateral;
            }

            return ShapeType.Unknown;
        }
    }
}
