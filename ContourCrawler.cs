using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2
{
    public class ContourCrawler
    {
        private Point _a;
        private Point? _b;
        private Point? _c;
        private Point? _d;

        public ContourCrawler(Point a)
        {
            _a = a;
        }

        public Point Start { get { return _a; } }
        public Point End { 
            get 
            { 
                if (_d.HasValue)
                {
                    return _d.Value;
                } 
                else if (_c.HasValue)
                {
                    return _c.Value;
                }
                else if (_b.HasValue)
                {
                    return _b.Value;
                } else
                {
                    return _a;
                }
            } 
        }

        public double Angle 
        {
            get
            {
                return CalculateAngle(_a, End);
            }
        }

        public double Deviation
        {
            get
            {
                double a, b;

                if (_a == End || _b == End)
                {
                    return 0;
                } else if (_c == End)
                {
                    a = CalculateAngle(_a, _b.Value);
                    b = CalculateAngle(_b.Value, _c.Value);

                    return a - b;
                }

                a = CalculateAngle(_a, _b.Value);
                b = CalculateAngle(_c.Value, _d.Value);

                return a - b;
            }
        }

        public void Add(Point newPoint)
        {
            if (_d.HasValue)
            {
                _a = _b.Value;
                _b = _c;
                _c = _d;
                _d = newPoint;
                return;
            } 

            if (_a == End)
            {
                _b = newPoint;
            } else if (_b == End)
            {
                _c = newPoint;
            } else
            {
                _d = newPoint;
            }
        }

        private double CalculateAngle(Point a, Point b)
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
    }
}
