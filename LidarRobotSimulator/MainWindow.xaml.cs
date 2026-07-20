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
        private HashSet<(int, int)> scannedCells = new HashSet<(int, int)>();
        private List<Point> trajectory = new List<Point>();
        private const int CellSize = 40;
        private bool isPaused = false;
        private bool mapDrawn = false;
        private const double RobotBodyWidthRatio = 0.87;
        private const double RobotBodyHeightRatio = 1.14;
        private const double CollisionMargin = 0.4;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                map = GridMap.LoadFromFile("Maps/map1.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить карту: {ex.Message}");
                map = new GridMap(10, 6);
                map.GenerateSampleObstacles();
            }
            robot = new Robot(2, 2, 0);
            lidar = new Lidar(10, 72);

            trajectory.Add(new Point(robot.X, robot.Y));

            DrawScene();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isPaused)
                return;

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

            if (CheckCollision(robot.X, robot.Y, robot.Angle))
            {
                robot.SetPosition(oldX, oldY);
            }

            if (robot.X != oldX || robot.Y != oldY)
            {
                trajectory.Add(new Point(robot.X, robot.Y));
            }

            DrawScene();
            e.Handled = true;
        }

        private bool CheckCollision(double x, double y, double angle)
        {
            double buffer = 1.0;
            double halfWidth = RobotBodyWidthRatio / 2 * buffer;
            double halfHeight = RobotBodyHeightRatio / 2 * buffer;

            double radians = (angle + 90) * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            var corners = new (double dx, double dy)[]
            {
        (-halfWidth, -halfHeight),
        (halfWidth, -halfHeight),
        (-halfWidth, halfHeight),
        (halfWidth, halfHeight)
            };

            foreach (var corner in corners)
            {
                double worldX = x + corner.dx * cos - corner.dy * sin;
                double worldY = y + corner.dx * sin + corner.dy * cos;

                if (!map.IsInsideBounds(worldX, worldY) || !map.IsFree((int)worldX, (int)worldY))
                    return true;
            }

            return false;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;
            PauseButton.Content = isPaused ? "Продолжить" : "Пауза";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            robot = new Robot(2, 2, 0);
            scannedPoints.Clear();
            scannedCells.Clear();
            trajectory.Clear();
            trajectory.Add(new Point(robot.X, robot.Y));
            isPaused = false;
            PauseButton.Content = "Пауза";

            DrawScene();
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (robot != null)
            {
                robot.Speed = e.NewValue;
            }

            if (SpeedLabel != null)
            {
                SpeedLabel.Text = e.NewValue.ToString("F1");
            }
        }

        private void DrawScene()
        {
            if (!mapDrawn)
            {
                DrawMap();
                mapDrawn = true;
            }

            ClearDynamicElements();
            DrawScannedPoints();
            DrawTrajectory();
            DrawLidarRays();
            DrawRobot();
            UpdateScannedMap();
        }

        private void ClearDynamicElements()
        {
            for (int i = MainCanvas.Children.Count - 1; i >= 0; i--)
            {
                var element = MainCanvas.Children[i];
                if (element is Rectangle)
                    continue;

                MainCanvas.Children.Remove(element);
            }
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

            double bodyWidth = CellSize * RobotBodyWidthRatio;
            double bodyHeight = CellSize * RobotBodyHeightRatio;
            double wheelWidth = CellSize * 0.14;
            double wheelHeight = CellSize * 0.56;
            double hubRadius = CellSize * 0.12;

            var robotGroup = new Canvas
            {
                Width = bodyWidth,
                Height = bodyHeight
            };

            var wheelTopLeft = new Rectangle { Width = wheelWidth, Height = wheelHeight, Fill = Brushes.Black };
            Canvas.SetLeft(wheelTopLeft, -wheelWidth / 2);
            Canvas.SetTop(wheelTopLeft, -5);
            robotGroup.Children.Add(wheelTopLeft);

            var wheelTopRight = new Rectangle { Width = wheelWidth, Height = wheelHeight, Fill = Brushes.Black };
            Canvas.SetLeft(wheelTopRight, bodyWidth - wheelWidth / 2);
            Canvas.SetTop(wheelTopRight, -5);
            robotGroup.Children.Add(wheelTopRight);

            var body = new Rectangle
            {
                Width = bodyWidth,
                Height = bodyHeight,
                Fill = Brushes.SteelBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(body, 0);
            Canvas.SetTop(body, 0);
            robotGroup.Children.Add(body);

            var hubFront = new Ellipse { Width = hubRadius * 2, Height = hubRadius * 2, Fill = Brushes.DarkGray, Stroke = Brushes.Black, StrokeThickness = 1 };
            Canvas.SetLeft(hubFront, bodyWidth / 2 - hubRadius);
            Canvas.SetTop(hubFront, bodyHeight * 0.25 - hubRadius);
            robotGroup.Children.Add(hubFront);

            var hubBack = new Ellipse { Width = hubRadius * 2, Height = hubRadius * 2, Fill = Brushes.DarkGray, Stroke = Brushes.Black, StrokeThickness = 1 };
            Canvas.SetLeft(hubBack, bodyWidth / 2 - hubRadius);
            Canvas.SetTop(hubBack, bodyHeight * 0.7 - hubRadius);
            robotGroup.Children.Add(hubBack);

            robotGroup.RenderTransformOrigin = new Point(0.5, 0.5);
            robotGroup.RenderTransform = new RotateTransform(robot.Angle + 90);

            Canvas.SetLeft(robotGroup, centerX - bodyWidth / 2);
            Canvas.SetTop(robotGroup, centerY - bodyHeight / 2);

            MainCanvas.Children.Add(robotGroup);
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

        private void UpdateScannedMap()
        {
            var newPoints = lidar.ScanPoints(map, robot.X, robot.Y, robot.Angle);

            foreach (var point in newPoints)
            {
                var key = ((int)(point.X * 10), (int)(point.Y * 10));

                if (!scannedCells.Contains(key))
                {
                    scannedCells.Add(key);
                    scannedPoints.Add(point);
                }
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