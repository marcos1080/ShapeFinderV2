using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2.Data.Shapes
{
    public class Pentagon : Shape
    {
        private enum Types
        {
            Regular,
            Irregular,
        }

        public Pentagon(List<Point> contour,
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
                return "Pentagon";
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
                return Pixels.Count;
            }
        }
    }
}
