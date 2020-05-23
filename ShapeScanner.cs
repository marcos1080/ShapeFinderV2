using ShapeFinderV2.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Scan the pixel data of a shape in an image from the first detected
    /// pixel co-ordinate.
    /// </summary>
    public class ShapeScanner
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

        private readonly Bitmap _image;
        private readonly PixelMap _pixelMap;

        public ShapeScanner(Bitmap image, PixelMap pixelMap)
        {
            _image = image;
            _pixelMap = pixelMap;
        }

        /// <summary>
        /// Scan around a shape contour in an image and collect data.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ShapeData Scan(int x, int y)
        {
            var pixels = new PixelCollection(x, y);
            List<Point> contour = DefineContour(x, y, pixels);

            // Populate visited array.
            PopulatePixels(pixels);

            return new ShapeData()
            {
                Contour = contour,
                Pixels = pixels
            };
        }

        /// <summary>
        /// Crawl around the contour of a shape in an image.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixels"></param>
        /// <returns></returns>
        private List<Point> DefineContour(int x, int y, PixelCollection pixels)
        {
            // Define contour.
            var contour = new List<Point>();

            // Start from the end of the top line.
            while (_pixelMap.PixelInShape(x + 1, y))
            {
                x += 1;
            }

            var initial = new Point(x, y);

            // Initialise data.
            contour.Add(initial);
            pixels.AddPixel(initial);
            _pixelMap.SetVisited(x, y);

            // Used to hold the previous pixel co-ords.
            var last = initial;

            // Initialise current to be the next pixel along the edge.

            // Next pixel will be on the next line.
            y = last.Y + 1;

            // If line does not have a pixel to the bottom right then it must be below
            // initial pixel.
            x = _pixelMap.PixelInShape(last.X + 1, y) ? last.X + 1 : last.X;

            var current = new Point(x, y);

            // Crawl around contour until back at start.
            while (current != initial)
            {
                // Update data.
                contour.Add(current);
                pixels.AddPixel(current);
                _pixelMap.SetVisited(current.X, current.Y);

                // Find the position of the first empty pixel next to the previous one
                // in a clockwise direction.
                Zone initialSearchZone = FindSearchZone(current, last);

                // Search in a clockwise direction until the first shape pixel is found.
                Point found = FindNextPixel(initialSearchZone, current);

                // Increment
                last = current;
                current = found;
            }

            return contour;
        }

        /// <summary>
        /// Basic method of finding the first blank pixel in a clockwise
        /// direction from the previous pixel.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        private Zone FindSearchZone(Point current, Point last)
        {
            // Previous pixel to the left of the current one.
            if (last.X < current.X)
            {
                // Previous above current.
                if (last.Y < current.Y)
                {
                    return Zone.Top;
                }
                // Previous below current.
                if (last.Y > current.Y)
                {
                    return Zone.Left;
                }
                // Previous beside current.
                return Zone.TopLeft;
            }

            // Previous pixel to the right of the current one.
            if (last.X > current.X)
            {
                // Previous above current.
                if (last.Y < current.Y)
                {
                    return Zone.Right;
                }
                // Previous below current.
                if (last.Y > current.Y)
                {
                    return Zone.Bottom;
                }
                // Previous beside current.
                return Zone.BottomRight;
            }

            // Previous pixel to the above or below of the current one.

            // Previous pixel below current.
            if (last.Y > current.Y)
            {
                return Zone.BottomLeft;
            }

            // Previous pixel above current.
            return Zone.TopRight;
        }

        /// <summary>
        /// Iterate in a clockwise direction around the current pixel from the
        /// initial search zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="pixel"></param>
        /// <returns></returns>
        private Point FindNextPixel(Zone zone, Point pixel)
        {
            while (true)
            {
                if (zone == Zone.TopLeft && _pixelMap.PixelInShape(pixel.X - 1, pixel.Y - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y - 1);
                }
                else if (zone == Zone.Top && _pixelMap.PixelInShape(pixel.X, pixel.Y - 1))
                {
                    return new Point(pixel.X, pixel.Y - 1);
                }
                else if (zone == Zone.TopRight && _pixelMap.PixelInShape(pixel.X + 1, pixel.Y - 1))
                {
                    return new Point(pixel.X + 1, pixel.Y - 1);
                }
                else if (zone == Zone.Right && _pixelMap.PixelInShape(pixel.X + 1, pixel.Y))
                {
                    return new Point(pixel.X + 1, pixel.Y);
                }
                else if (zone == Zone.BottomRight && _pixelMap.PixelInShape(pixel.X + 1, pixel.Y + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y + 1);
                }
                else if (zone == Zone.Bottom && _pixelMap.PixelInShape(pixel.X, pixel.Y + 1))
                {
                    return new Point(pixel.X, pixel.Y + 1);
                }
                else if (zone == Zone.BottomLeft && _pixelMap.PixelInShape(pixel.X - 1, pixel.Y + 1))
                {
                    return new Point(pixel.X - 1, pixel.Y + 1);
                }
                else if (zone == Zone.Left && _pixelMap.PixelInShape(pixel.X - 1, pixel.Y))
                {
                    return new Point(pixel.X - 1, pixel.Y);
                }

                // Iterate.
                if (zone == Zone.Left)
                {
                    zone = Zone.TopLeft;
                }
                else
                {
                    zone++;
                }
            }
        }

        /// <summary>
        /// Scan each row of the shape to update shape data between the contour values..
        /// </summary>
        /// <param name="pixels"></param>
        private void PopulatePixels(PixelCollection pixels)
        {
            foreach (var row in pixels.Rows)
            {
                var keys = row.Value.Keys;
                int index = 0;
                int y = row.Key;

                while (keys[index] != keys[^1])
                {
                    int start = keys[index];
                    int end = keys[index + 1];

                    for (int x = start + 1; x < end; x++) 
                    {
                        if (!_pixelMap.PixelInShape(x, y))
                        {
                            break;
                        }

                        // Update data.
                        pixels.AddPixel(x, y);
                        _pixelMap.SetVisited(x, y);
                    }

                    index++;
                }
            }
        }
    }
}
