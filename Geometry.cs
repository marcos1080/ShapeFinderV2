using System;
using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Simple helper class for common geometric calculations.
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// Calculate the angle between 2 points.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double CalculateAngle(Point a, Point b)
        {
            int opposite = a.Y - b.Y;
            int adjacent = b.X - a.X;

            // Need to handle possible divide by 0 situ.
            if (adjacent == 0)
            {
                if (opposite == 0)
                {
                    return 0;
                }

                return opposite > 0 ? 90 : 270;
            }

            double angle = Math.Atan2(opposite, adjacent) * (180 / Math.PI);

            return angle < 0 ? angle + 360 : angle;
        }

        /// <summary>
        /// Calculate the difference in angle between two lines.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double AngleDifference(Line a, Line b)
        {
            double angle = a.Angle - b.Angle;
            return angle < 0 ? angle + 360 : angle;
        }

        /// <summary>
        /// Calculate the distance between two points.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double CalculateDistance(Point a, Point b)
        {
            // Use Pythagoras Theorum
            double x = Math.Pow(a.X - b.X, 2);
            double y = Math.Pow(a.Y - b.Y, 2);

            return Math.Sqrt(x + y);
        }

        /// <summary>
        /// FInd the co-ordinate where two lines intersect.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static Point FindIntersection(Line line1, Line line2)
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

            if (delta == 0)
            {
                throw new Exception("Lines are parallel");
            }

            int x = Convert.ToInt32((b2 * c1 - b1 * c2) / delta);
            int y = Convert.ToInt32((a1 * c2 - a2 * c1) / delta);

            return new Point(x, y);
        }
    }
}
