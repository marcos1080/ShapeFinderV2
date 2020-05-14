using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    public class Shape
    {
        // Angle threshold that indicates a corner.
        private readonly double _threshold;
        private readonly Dictionary<int, List<int>> _pixels;

        public Shape(List<Point> contour, Dictionary<int, List<int>> pixels, double threshold)
        {
            Contour = contour;
            _pixels = pixels;
            _threshold = threshold;

            // Process Contour.
            Lines = FindLines();
            Corners = FindCorners(Lines);
            InferShape();
        }

        public List<Point> Contour { get; private set; }
        public List<Corner> Corners { get; private set; }
        public List<Line> Lines { get; private set; }
        public int Top { get; private set; }
        public int Right { get; private set; }
        public int Bottom { get; private set; }
        public int Left { get; private set; }
        public int Width
        {
            get { return Right - Left; }
        }
        public int Height
        {
            get { return Bottom - Top; }
        }
        public Point Center
        {
            get
            {
                int x = Convert.ToInt32(Math.Ceiling(Width / 2.0 + Left));
                int y = Convert.ToInt32(Math.Ceiling(Height / 2.0 + Top));

                return new Point(x, y);
            }
        }
        public string Type { get; private set; }
        public string Confidence { get; private set; }
        public int PixelCount
        {
            get
            {
                int count = 0;
                foreach (List<int> line in _pixels.Values)
                {
                    count += line.Count;
                }

                return count;
            }
        }

        private List<Line> FindLines()
        {
            // Init boundaries.
            Top = Contour[0].Y;
            Bottom = Contour[0].Y;
            Left = Contour[0].X;
            Right = Contour[0].X;

            // Sampling interval.
            int interval = Math.Max(3, Convert.ToInt32(Contour.Count * 0.02));

            var lines = new List<Line>();
            var currentLine = new Line(Contour[1]);
            var crawler = new ContourCrawler(Contour[0]);
            int distance = interval;

            while (distance < Contour.Count)
            {
                Point currentPoint = Contour[distance];

                // Adjust the bounds.
                // Using this method as a circle may only yield 1 line.
                Right = Math.Max(Right, currentPoint.X);
                Left = Math.Min(Left, currentPoint.X);
                Bottom = Math.Max(Bottom, currentPoint.Y);

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
                    crawler = new ContourCrawler(Contour[distance]);
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
            if (lines.Count == 1 && currentLine.End != Contour[^1])
            {
                currentLine.Add(Contour[^1]);
            }

            return lines;
        }

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
                corner = FindIntersection(a, b);

                corners.Add(new Corner()
                {
                    Coordinate = corner,
                    Angle = AngleDifference(a, b)
                });
            }

            // Compare first and last lines to get final corner.
            a = lines[^1];
            b = lines[0];
            corner = FindIntersection(a, b);

            corners.Add(new Corner()
            {
                Coordinate = corner,
                Angle = AngleDifference(a, b)
            });

            // Add corner co-ordinates to the corresponding lines, if not
            // present already.
            for (int i = 0; i < corners.Count; i++)
            {
                a = Lines[i];
                b = i == corners.Count - 1 ? Lines[0] : Lines[i + 1];

                a.AdjustEnd(corners[i].Coordinate);
                b.AdjustStart(corners[i].Coordinate);
            }

            return corners;
        }

        private void InferShape()
        {
            // Circle
            if (Lines.Count == 1 || Corners.Count > 4)
            {
                ProcessCircle();
                return;
            }

            if (Corners.Count == 3)
            {
                ProcessTriangle();
                return;
            }

            if (Corners.Count == 4)
            {
                ProcessQuad();
                return;
            }
        }

        private Point FindIntersection(Line line1, Line line2)
        {
            Point s1 = line1.Start;
            Point e1 = line1.End;
            Point s2 = line2.Start;
            Point e2 = line2.End;

            float a1 = e1.Y - s1.Y;
            float b1 = s1.X - e1.X;
            float c1 = a1 * s1.X + b1 * s1.Y;

            float a2 = e2.Y - s2.Y;
            float b2 = s2.X - e2.X;
            float c2 = a2 * s2.X + b2 * s2.Y;

            float delta = a1 * b2 - a2 * b1;

            int x = Convert.ToInt32((b2 * c1 - b1 * c2) / delta);
            int y = Convert.ToInt32((a1 * c2 - a2 * c1) / delta);

            return new Point(x, y);
        }

        private double AngleDifference(Line a, Line b)
        {
            double angle = a.Angle - b.Angle;
            return angle < 0 ? angle + 360 : angle;
        }

        private void ProcessCircle()
        {
            if (Lines.Count == 1 || Lines.Count > 4)
            {
                Type = "Circle";
            }

            // Calculate center.
            int x = Convert.ToInt32(Math.Ceiling((Right - Left) / 2.0 + Left));
            int y = Convert.ToInt32(Math.Ceiling((Bottom - Top) / 2.0 + Top));
            int radius = x - Left;

            // Perform a simple perimeter check.
            double degrees = 0;
            double varianceLimit = 3;
            bool OK = true;

            while (degrees < 360 && OK)
            {
                // Test just inside and outside of perimeter.

                // Outside
                int testX = x + Convert.ToInt32(Math.Round((radius + varianceLimit) * Math.Cos(degrees * Math.PI / 180), 1, 0));
                int testY = y + Convert.ToInt32(Math.Round((radius + varianceLimit) * Math.Sin(degrees * Math.PI / 180), 1, 0));

                if (_pixels.ContainsKey(testY) && _pixels[testY].Contains(testX))
                {
                    OK = false;
                }

                // Inside
                testX = x + Convert.ToInt32(Math.Round((radius - varianceLimit) * Math.Cos(degrees * Math.PI / 180), 1, 0));
                testY = y + Convert.ToInt32(Math.Round((radius - varianceLimit) * Math.Sin(degrees * Math.PI / 180), 1, 0));

                if (!(_pixels.ContainsKey(testY) && _pixels[testY].Contains(testX)))
                {
                    OK = false;
                }

                degrees += 30;
            }

            // Perimeter is within limits.
            if (OK)
            {
                Confidence = "Excellent";
                return;
            }

            // Perimeter check failed, perform other checks.
            double area = 2 * Math.PI * radius;
            double percentageDiff = area / PixelCount;

            double perimeter = Lines[0].LengthFromAccumulation;
            double calculatedPerimeter = 2 * Math.PI * radius;
            double perimeterDiff = Math.Abs(perimeter - calculatedPerimeter);

            if (perimeterDiff < 10 && percentageDiff < 0.05)
            {
                Confidence = "OK";
            }
            else
            {
                Confidence = "Bad";
            }
        }

        private void ProcessTriangle()
        {
            if (Lines.Count == 3)
            {
                Type = "Triangle";
            }

            SetConfidence();
        }

        private void ProcessQuad()
        {
            bool rectangle = true; ;

            foreach (Corner corner in Corners)
            {
                if (corner.Angle < 90 - 2 || corner.Angle > 90 + 2)
                {
                    rectangle = false;
                }
            }

            if (rectangle)
            {
                double adjacentDiff = Math.Abs(Lines[0].LengthBetweenPoints - Lines[1].LengthBetweenPoints);
                if (adjacentDiff > 2)
                {
                    Type = "Rectangle";
                } else
                {
                    Type = "Square";
                }
            } else
            {
                Type = "Quadralateral";
            }

            SetConfidence();
        }

        private void SetConfidence()
        {
            double maxVariance = 0;

            // Confidence will be effected by how much variation was encountered
            // within each line angle.
            foreach (Line line in Lines)
            {
                if (line.AvVariance > maxVariance)
                {
                    maxVariance = line.AvVariance;
                }
            }

            if (maxVariance < 4)
            {
                Confidence = "Excellent";
            }
            else if (maxVariance < 8)
            {
                Confidence = "OK";
            }
            else
            {
                Confidence = "Bad";
            }
        }
    }
}
