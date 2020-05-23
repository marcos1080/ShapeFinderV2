using System.Drawing;

namespace ShapeFinderV2
{
    /// <summary>
    /// Class that measures the rate of change between points.
    /// Can measure rate of change between 3 - 4 points.
    /// </summary>
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

        /// <summary>
        /// Start point of crawler along line.
        /// </summary>
        public Point Start { get { return _a; } }

        /// <summary>
        /// End point of crawler along line.
        /// The length of the line segment can be between 1 and 4 points.
        /// </summary>
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

        /// <summary>
        /// Angle between the start and end points.
        /// </summary>
        public double Angle 
        {
            get
            {
                return Geometry.CalculateAngle(_a, End);
            }
        }

        /// <summary>
        /// Calculate the difference in angle between 2 line segments.
        /// </summary>
        public double Deviation
        {
            get
            {
                double a, b, result;

                // If the line segment measured is less than 3 points there will be no deviation.
                if (_a == End || _b == End)
                {
                    return 0;
                } 
                
                // Only 3 points present. Can measure deviation but not as accurate as 4.
                if (_c == End)
                {
                    a = Geometry.CalculateAngle(_a, _b.Value);
                    b = Geometry.CalculateAngle(_b.Value, _c.Value);

                    return AngleDiff(a, b);
                }

                // 4 points present. will give best results.
                a = Geometry.CalculateAngle(_a, _b.Value);
                b = Geometry.CalculateAngle(_c.Value, _d.Value);

                return AngleDiff(a, b);
            }
        }

        /// <summary>
        /// Need to adjust the difference to account for direction.
        /// Must be within range of -180 - 180, not 0 - 360.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double AngleDiff(double a, double b)
        {
            double result = a - b;

            if (result > 180)
            {
                result = 360 - result;
            }

            return result;
        }

        /// <summary>
        /// Add a new point to be measured.
        /// If the number of points present is 4 then the oldest one gets
        /// removed and the others shuffle down, leaving room for the new one at the front.
        /// </summary>
        /// <param name="newPoint"></param>
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
    }
}
