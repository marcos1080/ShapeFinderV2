using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2
{
    public class Line
    {
        private Point _prev;
        private double _prevVariance = 0;
        private double _prevAngle = 0;
        private double _totalVariance = 0;
        private int _varianceCount = 0;

        public Line(Point initial)
        {
            Start = initial;
            _prev = initial;
            End = initial;
        }

        public Point Start { get; set; }
        public Point End { get; set; }
        public double MaxVariance { get; private set; } = 0;
        public double AvVariance
        {
            get {  return _totalVariance / _varianceCount; }
        }

        public double Angle
        {
            get { return CalculateAngle(Start, End); }
        }

        public double LengthBetweenPoints
        {
            get
            {
                return CalculateDistance(Start, End);
            }
        }

        public double LengthFromAccumulation { get; private set; } = 0;

        public void AdjustStart(Point p)
        {
            if (p == Start)
            {
                return;
            }

            double angle = CalculateAngle(p, Start);
            double variance = angle - _prevAngle;
            if (Math.Abs(variance) > MaxVariance)
            {
                MaxVariance = Math.Abs(variance);
            }

            LengthFromAccumulation += CalculateDistance(p, Start);
            _totalVariance += variance;
            _varianceCount++;
            Start = p;
        }

        public void AdjustEnd(Point p)
        {
            if (p == End)
            {
                return;
            }

            double angle = CalculateAngle(End, p);
            double variance = angle - _prevAngle;
            if (Math.Abs(variance) > MaxVariance)
            {
                MaxVariance = Math.Abs(variance);
            }

            LengthFromAccumulation += CalculateDistance(End, p);
            _totalVariance += variance;
            _varianceCount++;
            End = p;
        }

        public void Add(Point p)
        {
            if (Start == End)
            {
                // Calculate the first angle.
                _prevAngle = CalculateAngle(Start, p);
            }
            else
            {
                double angle = CalculateAngle(End, p);
                double variance = angle - _prevAngle;
                if (Math.Abs(variance) > MaxVariance)
                {
                    MaxVariance = Math.Abs(variance);
                }

                _prevAngle = angle;
                _prevVariance = variance;
                _totalVariance += variance;
                _varianceCount++;
            }

            // Increase distance.
            LengthFromAccumulation += CalculateDistance(End, p);

            _prev = End;
            End = p;
        }

        public void RemoveLast()
        {
            LengthFromAccumulation -= CalculateDistance(_prev, End);
            _totalVariance -= _prevVariance;
            _varianceCount--;
            End = _prev;
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

        private double CalculateDistance(Point a, Point b)
        {
            double x = Math.Pow(a.X - b.X, 2);
            double y = Math.Pow(a.Y - b.Y, 2);

            return Math.Sqrt(x + y);
        }
    }
}
