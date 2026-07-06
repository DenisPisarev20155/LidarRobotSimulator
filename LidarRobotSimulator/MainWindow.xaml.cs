using LidarRobotSimulator.Models;
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
        public MainWindow()
        {
            var map = new GridMap(10, 6);
            map.GenerateSampleObstacles();
            System.Diagnostics.Debug.WriteLine($"Карта {map.Width}x{map.Height} создана");

            var robot = new Robot(0, 0);
            robot.MoveForward();
            robot.TurnRight();
            robot.MoveForward();
            System.Diagnostics.Debug.WriteLine($"Робот на позиции X={robot.X:F2}, Y={robot.Y:F2}, угол={robot.Angle}");

            bool insideBounds = map.IsInsideBounds(robot.X, robot.Y);
            System.Diagnostics.Debug.WriteLine($"Робот внутри карты: {insideBounds}");
        }
    }
}