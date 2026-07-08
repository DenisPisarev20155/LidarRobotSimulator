using System;
using System.Collections.Generic;
namespace LidarRobotSimulator.Models
{
    public class Lidar
    {
        public double MaxRange { get; set; }
        public int RayCount { get; set; }
        public Lidar(double maxRange = 10, int rayCount = 36)
        {
            MaxRange = maxRange;
            RayCount = rayCount;
        }

        public List<double> Scan(GridMap map, double robotX, double robotY, double robotAngle)
        {
            var distances = new List<double>();
            double angleStep = 360.0 / RayCount;

            for (int i = 0; i < RayCount; i++)
            {
                double rayAngle = robotAngle + i * angleStep;
                double distance = CastRay(map, robotX, robotY, rayAngle);
                distances.Add(distance);
            }

            return distances;
        }

        private double CastRay(GridMap map, double startX, double startY, double angleDegrees)
        {
            double radians = angleDegrees * Math.PI / 180.0;
            double dx = Math.Cos(radians);
            double dy = Math.Sin(radians);

            double step = 0.1;
            double distance = 0;

            while (distance < MaxRange)
            {
                double checkX = startX + dx * distance;
                double checkY = startY + dy * distance;

                if (!map.IsInsideBounds(checkX, checkY))
                    return distance;

                if (!map.IsFree((int)checkX, (int)checkY))
                    return distance;

                distance += step;
            }

            return MaxRange;
        }
    }
}