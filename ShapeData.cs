using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Text;

namespace ShapeFinderV2
{
    
    public class ShapeData
    {
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

        private readonly int _initX;
        public Dictionary<int, List<int>> Pixels { get; private set; }
        public Point LineStart { get; private set; }
        public Point LineEnd { get; private set; }
        public List<Point> Contour { get; private set; }

        public ShapeData(int x, int y)
        {
            _initX = x;
            Top = y;
            Pixels = new Dictionary<int, List<int>>();
            Left = x;
            Right = x;
            Area = 0;
            Contour = new List<Point>();
        }

        public int Top { get; }
        public int Bottom { get { return Top + Pixels.Count - 1; } }
        public int Left { get; private set; }
        public int Right { get; private set; }
        public int Area { get; private set; }

        public void AddPixel(int x, int y)
        {
            if (!Pixels.ContainsKey(y))
            {
                Pixels.Add(y, new List<int>());
                LineStart = new Point(x, y);
                if (x < Left)
                {
                    Left = x;
                }
            }

            Pixels[y].Add(x);
            LineEnd = new Point(x, y);
            if (x > Right)
            {
                Right = x;
            }
            Area++;
        }

        public void Finalise()
        {
            // Define contour.
            DefineContour();
        }

        private void DefineContour() {
            var initial = new Point(Pixels[Top][^1], Top);
            Contour.Add(initial);

            var last = initial;
            int x = last.X == Right ? last.X : last.X + 1;
            int y = last.Y + 1;
            var current = new Point(x, y);

            // Crawl around contour until back at start.
            while (current != initial) 
            {
                Contour.Add(current);
                Zone initialSearchZone = FindSearchZone(current, last);
                Point found = FindNextPixel(initialSearchZone, current);
                last = current;
                current = found;
            }
        }

        private Zone FindSearchZone(Point current, Point last)
        {
            if (last.X < current.X)
            {
                if (last.Y < current.Y)
                {
                    return Zone.Top;
                }
                if (last.Y > current.Y)
                {
                    return Zone.Left;
                }
                return Zone.TopLeft;
            }

            if (last.X > current.X)
            {
                if (last.Y < current.Y)
                {
                    return Zone.Right;
                }
                if (last.Y > current.Y)
                {
                    return Zone.Bottom;
                }
                return Zone.BottomRight;
            }

            if (last.Y > current.Y)
            {
                return Zone.BottomLeft;
            }

            return Zone.TopRight;
        }

        private Point FindNextPixel(Zone zone, Point pixel)
        {
            Zone endZone = zone;

            if (endZone == Zone.TopLeft)
            {
                endZone = Zone.Left;
            }
            else
            {
                endZone--;
            }

            while (zone != endZone)
            {
                if (zone == Zone.TopLeft && Pixels.ContainsKey(pixel.Y - 1) 
                    && Pixels[pixel.Y - 1].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y - 1);
                }
                else if (zone == Zone.Top && Pixels.ContainsKey(pixel.Y - 1) 
                    && Pixels[pixel.Y - 1].Contains(pixel.X))
                {
                    return new Point(pixel.X, pixel.Y - 1);
                }
                else if (zone == Zone.TopRight && Pixels.ContainsKey(pixel.Y - 1) 
                    && Pixels[pixel.Y - 1].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y - 1);
                }
                else if (zone == Zone.Right && Pixels[pixel.Y].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y);
                }
                else if (zone == Zone.BottomRight && Pixels.ContainsKey(pixel.Y + 1) 
                    && Pixels[pixel.Y + 1].Contains(pixel.X + 1))
                {
                    return new Point(pixel.X + 1, pixel.Y + 1);
                }
                else if (zone == Zone.Bottom && Pixels.ContainsKey(pixel.Y + 1) 
                    && Pixels[pixel.Y + 1].Contains(pixel.X))
                {
                    return new Point(pixel.X, pixel.Y + 1);
                }
                else if (zone == Zone.BottomLeft && Pixels.ContainsKey(pixel.Y + 1) 
                    && Pixels[pixel.Y + 1].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y + 1);
                }
                else if (zone == Zone.Left && Pixels[pixel.Y].Contains(pixel.X - 1))
                {
                    return new Point(pixel.X - 1, pixel.Y);
                }

                if (zone == Zone.Left)
                {
                    zone = Zone.TopLeft;
                } else
                {
                    zone++;
                }
            }

            return new Point(0, 0);
        }
    }
}
