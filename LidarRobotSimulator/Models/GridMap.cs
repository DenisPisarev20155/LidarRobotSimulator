using System;
using System.IO;

namespace LidarRobotSimulator.Models
{
    public class GridMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private MapCell[,] cells;

        public GridMap(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new MapCell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new MapCell(x, y);
                }
            }
        }

        public MapCell GetCell(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;

            return cells[x, y];
        }

        public bool IsFree(int x, int y)
        {
            var cell = GetCell(x, y);
            return cell != null && !cell.IsObstacle;
        }

        public bool IsInsideBounds(double x, double y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public void SetObstacle(int x, int y, bool value)
        {
            var cell = GetCell(x, y);
            if (cell != null)
                cell.IsObstacle = value;
        }

        public void GenerateSampleObstacles()
        {
            for (int x = 3; x < 7; x++)
            {
                SetObstacle(x, 3, true);
            }

            for (int y = 4; y < 8; y++)
            {
                SetObstacle(8, y, true);
            }
        }

        public static GridMap LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Файл карты не найден: {path}");
            }

            string[] lines = File.ReadAllLines(path);
            int height = lines.Length;
            int width = lines.Length > 0 ? lines[0].Length : 0;

            if (width == 0 || height == 0)
            {
                throw new InvalidDataException("Файл карты пустой или имеет неверный формат");
            }

            var map = new GridMap(width, height);

            for (int y = 0; y < height; y++)
            {
                string line = lines[y];
                for (int x = 0; x < width && x < line.Length; x++)
                {
                    bool isObstacle = line[x] == '#';
                    map.SetObstacle(x, y, isObstacle);
                }
            }

            return map;
        }

        public void SaveToFile(string path)
        {
            var writer = new StreamWriter(path);

            for (int y = 0; y < Height; y++)
            {
                var line = new char[Width];
                for (int x = 0; x < Width; x++)
                {
                    line[x] = cells[x, y].IsObstacle ? '#' : '.';
                }
                writer.WriteLine(new string(line));
            }

            writer.Close();
        }
    }
}