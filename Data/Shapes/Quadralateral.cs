using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2
{
    /// <summary>
    /// Quadralateral shape.
    /// </summary>
    public class Quadralateral : Shape
    {
        private enum Types
        {
            Rectangle,
            Square,
            Parallelogram,
            Rhombus,
            Trapezoid,
            Kite,
            Quadralateral
        }

        public Quadralateral(List<Point> contour,
            PixelCollection pixels,
            List<Line> lines,
            List<Corner> corners) : base(contour, pixels, lines, corners)
        {
        }

        /// <summary>
        /// Type of quadralateral.
        /// </summary>
        public override string Type 
        {
            get
            {
                // Angle threshold.
                double aThreshold = 3;

                // Length threshold.
                double lThreshold = 2;

                // Lengths
                double a = Lines[0].LengthBetweenPoints;
                double b = Lines[1].LengthBetweenPoints;
                double c = Lines[2].LengthBetweenPoints;
                double d = Lines[3].LengthBetweenPoints;

                // Angles
                double aa = Corners[3].Angle;
                double ba = Corners[0].Angle;
                double ca = Corners[1].Angle;
                double da = Corners[2].Angle;

                Types type = Types.Quadralateral;

                // Both opposite angles are equal.
                if (Math.Abs(aa - ca) < aThreshold && Math.Abs(ba - da) < aThreshold)
                {
                    // Possible Parallelogram, Rectangle, Square or rhombus.
                    if (aa > 90 - aThreshold && aa < 90 + aThreshold)
                    {
                        // Rectangle or square.
                        if (Math.Abs(a - b) < lThreshold)
                        {
                            type = Types.Square;
                        }
                        else
                        {
                            type = Types.Rectangle;
                        }
                    }
                    else
                    {
                        // Parallelogram or rhombus.
                        if (Math.Abs(a - b) < lThreshold)
                        {
                            type = Types.Rhombus;
                        }
                        else
                        {
                            type = Types.Parallelogram;
                        }
                    }
                }
                else
                {
                    // Possible Trapezoid, Kite or Quadralateral.
                    double line1Angle = Lines[0].Angle >= 180 ? Lines[0].Angle - 180 : Lines[0].Angle;
                    double line2Angle = Lines[1].Angle >= 180 ? Lines[1].Angle - 180 : Lines[1].Angle;
                    double line3Angle = Lines[2].Angle >= 180 ? Lines[2].Angle - 180 : Lines[2].Angle;
                    double line4Angle = Lines[3].Angle >= 180 ? Lines[3].Angle - 180 : Lines[3].Angle;

                    // Check if either opposite sides are parallel.
                    if (Math.Abs(line1Angle - line3Angle) < aThreshold || Math.Abs(line2Angle - line4Angle) < aThreshold)
                    {
                        type = Types.Trapezoid;
                    }
                    // Check if either opposite angles are equal.
                    else if (Math.Abs(aa - ca) < aThreshold || Math.Abs(ba - da) < aThreshold)
                    {
                        type = Types.Kite;
                    }

                    // If these checks fail then quadralateral.
                }

                return type.ToString();
            }
        }

        /// <summary>
        /// Determine how confinent the inference has been.
        /// </summary>
        public override string Confidence { get { return base.Confidence; } }

        /// <summary>
        /// Area of any quadralateral..
        /// </summary>
        public override double Area 
        {
            get
            {
                // Line lengths
                double a = Lines[0].LengthBetweenPoints;
                double b = Lines[1].LengthBetweenPoints;
                double c = Lines[2].LengthBetweenPoints;
                double d = Lines[3].LengthBetweenPoints;

                // Angles.
                double A = Corners[^1].Angle * Math.PI / 180;
                double C = Corners[1].Angle * Math.PI / 180;
                double area = 0.5 * a * d * Math.Sin(A) + 0.5 * b * c * Math.Sin(C);

                return area;
            }
        }
    }
}
