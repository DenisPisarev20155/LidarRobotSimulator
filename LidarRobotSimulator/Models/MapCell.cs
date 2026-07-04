namespace LidarRobotSimulator.Models
{
    public class MapCell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsObstacle { get; set; }

        public MapCell(int x, int y, bool isObstacle = false)
        {
            X = x;
            Y = y;
            IsObstacle = isObstacle;
        }
    }
}