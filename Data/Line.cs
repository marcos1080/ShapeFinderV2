using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2
{
    /// <summary>
    /// Class to hold all information about a line.
    /// </summary>
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

        /// <summary>
        /// Start co-ordinate.
        /// </summary>
        public Point Start { get; set; }

        /// <summary>
        /// End co-ordinate.
        /// </summary>
        public Point End { get; set; }

        /// <summary>
        /// The largest deviation from the pure line.
        /// </summary>
        public double MaxVariance { get; private set; } = 0;

        /// <summary>
        /// The amount of deviation from the pure line.
        /// Essentially indicates how straight the line is.
        /// </summary>
        public double AvVariance
        {
            get {  return _totalVariance / _varianceCount; }
        }

        /// <summary>
        /// Calculated angle of the line from start to finish.
        /// </summary>
        public double Angle
        {
            get { return Geometry.CalculateAngle(Start, End); }
        }

        /// <summary>
        /// The calculated lenght between the start and end.
        /// </summary>
        public double LengthBetweenPoints
        {
            get
            {
                return Geometry.CalculateDistance(Start, End);
            }
        }

        /// <summary>
        /// Accumulation of lengths between all the points that make up the line.
        /// </summary>
        public double LengthFromAccumulation { get; private set; } = 0;

        /// <summary>
        /// Add a new point to the start of the line.
        /// </summary>
        /// <param name="p">New point</param>
        public void AdjustStart(Point p)
        {
            if (p == Start)
            {
                return;
            }

            double angle = Geometry.CalculateAngle(p, Start);
            double variance = angle - _prevAngle;
            if (Math.Abs(variance) > MaxVariance)
            {
                MaxVariance = Math.Abs(variance);
            }

            // Adjust data.
            LengthFromAccumulation += Geometry.CalculateDistance(p, Start);
            _totalVariance += variance;
            _varianceCount++;
            Start = p;
        }

        /// <summary>
        /// Add a new point to the end of the line.
        /// </summary>
        /// <param name="p">New point</param>
        public void AdjustEnd(Point p)
        {
            if (p == End)
            {
                return;
            }

            double angle = Geometry.CalculateAngle(End, p);
            double variance = angle - _prevAngle;
            if (Math.Abs(variance) > MaxVariance)
            {
                MaxVariance = Math.Abs(variance);
            }

            // Adjust data.
            LengthFromAccumulation += Geometry.CalculateDistance(End, p);
            _totalVariance += variance;
            _varianceCount++;
            End = p;
        }

        /// <summary>
        /// Add next point in the line.
        /// </summary>
        /// <param name="p"></param>
        public void Add(Point p)
        {
            if (Start == End)
            {
                // Calculate the first angle.
                _prevAngle = Geometry.CalculateAngle(Start, p);
            }
            else
            {
                // Compare the angle of this point and the end, to the last calculated
                // angle. The difference will indicate the straightness of the line.
                double angle = Geometry.CalculateAngle(End, p);
                double variance = angle - _prevAngle;
                if (Math.Abs(variance) > MaxVariance)
                {
                    MaxVariance = Math.Abs(variance);
                }

                // Update data.
                _prevAngle = angle;
                _prevVariance = variance;
                _totalVariance += variance;
                _varianceCount++;
            }

            // Increase distance.
            LengthFromAccumulation += Geometry.CalculateDistance(End, p);

            _prev = End;
            End = p;
        }

        /// <summary>
        /// Remove the last point from the line.
        /// Currently this can only be done once as only the data for the second
        /// last point is kept in the _prev variable.
        /// This is for adjustment purposes.
        /// </summary>
        public void RemoveLast()
        {
            LengthFromAccumulation -= Geometry.CalculateDistance(_prev, End);
            _totalVariance -= _prevVariance;
            _varianceCount--;
            End = _prev;
        }
    }
}
