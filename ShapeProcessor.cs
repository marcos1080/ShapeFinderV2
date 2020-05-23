using System;
using System.Collections.Generic;
using System.Drawing;

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

        /// <summary>
        /// Decide what type of shape it is based on number of lines and corners.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Shape InferShape(ShapeData data)
        {
            // Process Contour.
            List<Line> lines = FindLines(data.Contour);
            List<Corner> corners = FindCorners(lines);
            ShapeType type = InferShape(lines, corners, data.Pixels);

            switch (type)
            {
                case ShapeType.Circle:
                    return new Circle(data.Contour, data.Pixels, lines, corners);
                case ShapeType.Triangle:
                    return new Triangle(data.Contour, data.Pixels, lines, corners);
                case ShapeType.Quadralateral:
                    return new Quadralateral(data.Contour, data.Pixels, lines, corners);
                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// Crawl around contour and detect straight lines from corners.
        /// </summary>
        /// <returns></returns>
        private List<Line> FindLines(List<Point> contour)
        {
            // Sampling interval.
            int interval = Math.Max(3, Convert.ToInt32(contour.Count * 0.02));

            var lines = new List<Line>();
            var currentLine = new Line(contour[1]);
            var crawler = new ContourCrawler(contour[0]);
            int distance = interval;

            while (distance < contour.Count)
            {
                Point currentPoint = contour[distance];

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
