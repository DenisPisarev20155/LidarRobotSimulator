namespace LidarRobotSimulator.Models
{
    public class ScannedPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public ScannedPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}