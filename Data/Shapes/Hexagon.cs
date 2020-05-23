using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2.Data.Shapes
{
    class Hexagon : Shape
    {
        private enum Types
        {
            Regular,
            Irregular,
        }

        public Hexagon(List<Point> contour,
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
                return "Hexagon";
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
