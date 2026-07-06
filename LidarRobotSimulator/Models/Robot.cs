using System;

namespace LidarRobotSimulator.Models
{
    public class Robot
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Angle { get; private set; }
        public double Speed { get; set; }
        public double RotationSpeed { get; set; }

        public Robot(double startX, double startY, double startAngle = 0)
        {
            X = startX;
            Y = startY;
            Angle = startAngle;
            Speed = 1.0;
            RotationSpeed = 5.0;
        }

        public void MoveForward()
        {
            double radians = Angle * Math.PI / 180.0;
            X += Speed * Math.Cos(radians);
            Y += Speed * Math.Sin(radians);
        }

        public void MoveBackward()
        {
            double radians = Angle * Math.PI / 180.0;
            X -= Speed * Math.Cos(radians);
            Y -= Speed * Math.Sin(radians);
        }

        public void TurnLeft()
        {
            Angle -= RotationSpeed;
            NormalizeAngle();
        }

        public void TurnRight()
        {
            Angle += RotationSpeed;
            NormalizeAngle();
        }

        public void SetPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        private void NormalizeAngle()
        {
            if (Angle < 0)
                Angle += 360;
            if (Angle >= 360)
                Angle -= 360;
        }
    }
}