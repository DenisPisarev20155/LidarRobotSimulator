using LidarRobotSimulator.Models;
using System;
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
        private const int CellSize = 40;

        public MainWindow()
        {
            InitializeComponent();

            map = GridMap.LoadFromFile("Maps/map1.txt");
            robot = new Robot(2, 2, 0);
            lidar = new Lidar(10, 36);

            DrawScene();
        }

        private void DrawScene()
        {
            MainCanvas.Children.Clear();
            DrawMap();
            DrawLidarRays();
            DrawRobot();
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
    }
}