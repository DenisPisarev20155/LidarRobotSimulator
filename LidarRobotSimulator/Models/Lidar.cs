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
                CastRay(map, robotX, robotY, rayAngle, out double distance);
                distances.Add(distance);
            }

            return distances;
        }

        public List<ScannedPoint> ScanPoints(GridMap map, double robotX, double robotY, double robotAngle)
        {
            var points = new List<ScannedPoint>();
            double angleStep = 360.0 / RayCount;

            for (int i = 0; i < RayCount; i++)
            {
                double rayAngle = robotAngle + i * angleStep;
                bool hit = CastRay(map, robotX, robotY, rayAngle, out double distance);

                if (hit)
                {
                    double radians = rayAngle * Math.PI / 180.0;
                    double pointX = robotX + Math.Cos(radians) * distance;
                    double pointY = robotY + Math.Sin(radians) * distance;

                    points.Add(new ScannedPoint(pointX, pointY));
                }
            }

            return points;
        }

        private bool CastRay(GridMap map, double startX, double startY, double angleDegrees, out double distance)
        {
            double radians = angleDegrees * Math.PI / 180.0;
            double dx = Math.Cos(radians);
            double dy = Math.Sin(radians);

            double step = 0.1;
            distance = 0;

            while (distance < MaxRange)
            {
                double checkX = startX + dx * distance;
                double checkY = startY + dy * distance;

                if (!map.IsInsideBounds(checkX, checkY))
                    return true;

                if (!map.IsFree((int)checkX, (int)checkY))
                    return true;

                distance += step;
            }

            distance = MaxRange;
            return false;
        }
    }
}