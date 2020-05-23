using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Triangle shape.
    /// </summary>
    public class Triangle : Shape
    {
        private enum Types
        {
            Equalateral,
            Isocoles,
            Scalene
        }

        private enum Angles
        {
            Obtuse,
            Acute,
            Right_Angle
        }

        public Triangle(List<Point> contour,
            PixelCollection pixels,
            List<Line> lines,
            List<Corner> corners) : base(contour, pixels, lines, corners)
        {
        }

        /// <summary>
        /// Type of triangle.
        /// </summary>
        public override string Type
        {
            get
            {
                // angle threshold for equal angle.
                double threshold = 3;

                double a = Corners[0].Angle;
                double b = Corners[1].Angle;
                double c = Corners[2].Angle;

                Types type = Types.Scalene;

                if (Math.Abs(a - b) < threshold)
                {
                    // Equal
                    if (Math.Abs(a - c) < threshold)
                    {
                        type = Types.Equalateral;
                    }
                    else
                    {
                        type = Types.Isocoles;
                    }
                }
                else
                {
                    if (Math.Abs(a - c) < threshold)
                    {
                        type = Types.Isocoles;
                    }
                }

                double largestAngle = Math.Max(a, b);
                largestAngle = Math.Max(largestAngle, c);

                Angles angle = Angles.Right_Angle;

                if (largestAngle > 90 + threshold)
                {
                    angle = Angles.Obtuse;
                }
                else if (largestAngle < 90 - threshold)
                {
                    angle = Angles.Acute;
                }

                return $"Triangle ({angle.ToString().Replace("_", " ")} {type})";
            }
        }

        /// <summary>
        /// Determine how confinent the inference has been.
        /// </summary>
        public override string Confidence { get { return base.Confidence; } }

        /// <summary>
        /// Area of any triangle.
        /// </summary>
        public override double Area
        {
            get 
            {
                // Herons Formula
                double a = Lines[0].LengthBetweenPoints;
                double b = Lines[1].LengthBetweenPoints;
                double c = Lines[2].LengthBetweenPoints;
                double s = (a + b + c) / 2;
                double area =  Math.Sqrt(s * (s - a) * (s - b) * (s - c));

                return area;
            }
        }
    }
}
