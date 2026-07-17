using LidarRobotSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LidarRobotSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GridMap map;
        private Robot robot;
        private Lidar lidar;
        private List<ScannedPoint> scannedPoints = new List<ScannedPoint>();
        private List<Point> trajectory = new List<Point>();
        private const int CellSize = 40;

        public MainWindow()
        {
            InitializeComponent();

            map = GridMap.LoadFromFile("Maps/map1.txt");
            robot = new Robot(2, 2, 0);
            lidar = new Lidar(10, 36);

            DrawScene();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double oldX = robot.X;
            double oldY = robot.Y;

            switch (e.Key)
            {
                case Key.Up:
                    robot.MoveForward();
                    break;
                case Key.Down:
                    robot.MoveBackward();
                    break;
                case Key.Left:
                    robot.TurnLeft();
                    break;
                case Key.Right:
                    robot.TurnRight();
                    break;
            }

            if (CheckCollision(robot.X, robot.Y))
            {
                robot.SetPosition(oldX, oldY);
            }

            if (robot.X != oldX || robot.Y != oldY)
            {
                trajectory.Add(new Point(robot.X, robot.Y));
            }

            DrawScene();
        }

        private bool CheckCollision(double x, double y)
        {
            double margin = 0.3;

            double[] checkX = { x - margin, x + margin };
            double[] checkY = { y - margin, y + margin };

            foreach (var cx in checkX)
            {
                foreach (var cy in checkY)
                {
                    if (!map.IsInsideBounds(cx, cy) || !map.IsFree((int)cx, (int)cy))
                        return true;
                }
            }

            return false;
        }

        private void DrawScene()
        {
            MainCanvas.Children.Clear();
            DrawMap();
            DrawLidarRays();
            DrawScannedPoints();
            DrawTrajectory();
            DrawRobot();
            UpdateScannedMap();
        }

        private void DrawMap()
        {
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var rect = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        Fill = map.IsFree(x, y) ? Brushes.White : Brushes.DimGray
                    };

                    Canvas.SetLeft(rect, x * CellSize);
                    Canvas.SetTop(rect, y * CellSize);
                    MainCanvas.Children.Add(rect);
                }
            }
        }

        private void DrawRobot()
        {
            double centerX = robot.X * CellSize;
            double centerY = robot.Y * CellSize;
            double radius = CellSize / 3.0;

            var ellipse = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = Brushes.Blue
            };

            Canvas.SetLeft(ellipse, centerX - radius);
            Canvas.SetTop(ellipse, centerY - radius);
            MainCanvas.Children.Add(ellipse);

            double radians = robot.Angle * Math.PI / 180.0;
            double lineEndX = centerX + Math.Cos(radians) * radius * 2;
            double lineEndY = centerY + Math.Sin(radians) * radius * 2;

            var directionLine = new Line
            {
                X1 = centerX,
                Y1 = centerY,
                X2 = lineEndX,
                Y2 = lineEndY,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            MainCanvas.Children.Add(directionLine);
        }

        private void DrawLidarRays()
        {
            var distances = lidar.Scan(map, robot.X, robot.Y, robot.Angle);
            double angleStep = 360.0 / lidar.RayCount;

            for (int i = 0; i < distances.Count; i++)
            {
                double rayAngle = robot.Angle + i * angleStep;
                double radians = rayAngle * Math.PI / 180.0;

                double startX = robot.X * CellSize;
                double startY = robot.Y * CellSize;
                double endX = startX + Math.Cos(radians) * distances[i] * CellSize;
                double endY = startY + Math.Sin(radians) * distances[i] * CellSize;

                var line = new Line
                {
                    X1 = startX,
                    Y1 = startY,
                    X2 = endX,
                    Y2 = endY,
                    Stroke = Brushes.OrangeRed,
                    StrokeThickness = 1
                };

                MainCanvas.Children.Add(line);
            }
        }

        private void DrawScannedPoints()
        {
            foreach (var point in scannedPoints)
            {
                var dot = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = Brushes.Green
                };

                Canvas.SetLeft(dot, point.X * CellSize - 2);
                Canvas.SetTop(dot, point.Y * CellSize - 2);
                MainCanvas.Children.Add(dot);
            }
        }

        private void UpdateScannedMap()
        {
            var newPoints = lidar.ScanPoints(map, robot.X, robot.Y, robot.Angle);

            foreach (var point in newPoints)
            {
                bool alreadyExists = scannedPoints.Exists(p =>
                    Math.Abs(p.X - point.X) < 0.02 && Math.Abs(p.Y - point.Y) < 0.3);

                if (!alreadyExists)
                {
                    scannedPoints.Add(point);
                }
            }
        }

        private void DrawTrajectory()
        {
            for (int i = 0; i < trajectory.Count - 1; i++)
            {
                var line = new Line
                {
                    X1 = trajectory[i].X * CellSize,
                    Y1 = trajectory[i].Y * CellSize,
                    X2 = trajectory[i + 1].X * CellSize,
                    Y2 = trajectory[i + 1].Y * CellSize,
                    Stroke = Brushes.Purple,
                    StrokeThickness = 2
                };

                MainCanvas.Children.Add(line);
            }
        }
    }
}