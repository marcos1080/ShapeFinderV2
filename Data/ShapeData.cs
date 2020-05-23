using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2
{
    public class ShapeData
    {
        public List<Point> Contour { get; set; }
        public PixelCollection Pixels { get; set; }
    }
}
