using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Circle shape
    /// </summary>
    public class Circle : Shape
    {
        public Circle(List<Point> contour,
            PixelCollection pixels,
            List<Line> lines,
            List<Corner> corners) : base(contour, pixels, lines, corners)
        {
        }

        /// <summary>
        /// Circle.
        /// </summary>
        public override string Type { get { return "Circle"; } }

        /// <summary>
        /// Radius of circle.
        /// </summary>
        public double Radius { 
            get 
            { 
                return Center.X - Pixels.Left;
            }
        }

        /// <summary>
        /// Area of circle.
        /// </summary>
        public override double Area
        {
            get
            {
                return 2 * Math.PI * Radius;
            }
        }

        /// <summary>
        /// Determine how confinent the inference has been.
        /// </summary>
        public override string Confidence
        {
            get
            {
                // Perform a simple perimeter check.
                double degrees = 0;
                double varianceLimit = 3;
                bool OK = true;

                while (degrees < 360 && OK)
                {
                    // Test just inside and outside of perimeter.

                    // Outside
                    int testX = Center.X + Convert.ToInt32(Math.Round((Radius + varianceLimit) * Math.Cos(degrees * Math.PI / 180), 1, 0));
                    int testY = Center.Y + Convert.ToInt32(Math.Round((Radius + varianceLimit) * Math.Sin(degrees * Math.PI / 180), 1, 0));

                    if (Pixels.Rows.ContainsKey(testY) && Pixels.Rows[testY].ContainsKey(testX))
                    {
                        OK = false;
                    }

                    // Inside
                    testX = Center.X + Convert.ToInt32(Math.Round((Radius - varianceLimit) * Math.Cos(degrees * Math.PI / 180), 1, 0));
                    testY = Center.Y + Convert.ToInt32(Math.Round((Radius - varianceLimit) * Math.Sin(degrees * Math.PI / 180), 1, 0));

                    if (!(Pixels.Rows.ContainsKey(testY) && Pixels.Rows[testY].ContainsKey(testX)))
                    {
                        OK = false;
                    }

                    degrees += 30;
                }

                // Perimeter is within limits.
                if (OK)
                {
                    return "Excellent";
                }

                // Perimeter check failed, perform other checks.
                double percentageDiff = Area / Pixels.Count;

                double perimeter = Lines[0].LengthFromAccumulation;
                double calculatedPerimeter = 2 * Math.PI * Radius;
                double perimeterDiff = Math.Abs(perimeter - calculatedPerimeter);

                if (perimeterDiff < 10 && percentageDiff < 0.05)
                {
                    return "OK";
                }
                else
                {
                    return "Bad";
                }
            }
        }
    }
}
