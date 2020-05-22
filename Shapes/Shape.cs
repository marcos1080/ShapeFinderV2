using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Class that holds the final shape data.
    /// </summary>
    public abstract class Shape
    {
        public Shape(List<Point> contour, 
            PixelCollection pixels,
            List<Line> lines,
            List<Corner> corners)
        {
            Contour = contour;
            Pixels = pixels;
            Lines = lines;
            Corners = corners;
        }

        public List<Point> Contour { get; private set; }
        public PixelCollection Pixels { get; private set; }
        public List<Corner> Corners { get; private set; }
        public List<Line> Lines { get; private set; }
        public int Top { get { return Pixels.Top; } }
        public int Right { get { return Pixels.Right; } }
        public int Bottom { get { return Pixels.Bottom; } }
        public int Left { get { return Pixels.Left; } }
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

        // Type of shape.
        public abstract string Type { get; }

        public abstract double Area { get; }

        // Rough approximation of area.
        public int PixelCount
        {
            get { return Pixels.Count; }
        }

        // How confident the inference is.
        public virtual string Confidence {
            get {
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
                    return "Excellent";
                }
                else if (maxVariance < 8)
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
