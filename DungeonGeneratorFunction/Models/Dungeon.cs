using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeHow.DungeonGenerator.Models
{
    interface IDungeon
    {
        int ySize { get; set; }
        int xSize { get; set; }
    }

    public class Dungeon : IDungeon
    {
        public int ySize { get; set; }
        public int xSize { get; set; }

        List<List<ITile>> Map { get; set; }

        public Dungeon ()
        {
            Map = new List<List<ITile>>();
        }

        internal static IDungeon CreateDungeon() { return CreateDungeon(100, 100); }

        internal static IDungeon CreateDungeon(int width, int height)
        {
            Dungeon dungeon = new Dungeon();

            for (int i = 0; i < width; i++)
            {
                dungeon.Map.Add(new List<ITile>());
                for (int j = 0; j < height; j++)
                {
                    dungeon.Map[i].Add(new Tile { X = i, Y = j });
                }
            }

            // Create first room in center
            dungeon.CreateRoom(width/2, height/2, 4, 4);

            return dungeon;
        }

        private void CreateRoom(int x, int y, int width, int height)
        {
            FillRect(x, y, width, height, TileType.Floor);
        }

        /// <summary>
        /// Sets a rectangle on the map to a specified TileType, starting from the bottom left corner of the position.
        /// </summary>
        private void FillRect(int x, int y, int width, int height, TileType type)
        {
            // Make sure x and y don't go outside the map
            if (x < 0 || y < 0) throw new ArgumentException("Ensure that x and y are not negative!");
            // If x or y plus requested rectangle size is outside map bounds, set to edge of map
            x = x + width > xSize ? width - xSize : x;
            y = y + height > ySize ? height - ySize : y;

            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    Map[i][j].TileType = type;
                }
            }
        }

        public override string ToString()
        {
            // Get the top left and bottom right of map and print the lines between them to a string
            StringBuilder sb = new StringBuilder();

            // Get cell with lowest X and highest Y
            ITile lowestX = Map.SelectMany((row, i) => row.Select((value, j) => new { i, j, value }))
                .Where(v => v.value.TileType != TileType.Empty)
                .OrderBy(x => x.value.X).ThenBy(x => x.value.Y)
                .First().value;
            ITile highestY = Map.SelectMany((row, i) => row.Select((value, j) => new { i, j, value }))
                .Where(v => v.value.TileType != TileType.Empty)
                .OrderByDescending(y => y.value.Y).ThenByDescending(y => y.value.X)
                .First().value;

            // Select right-most and upper-most ranges for loops
            int xLoopMax = Math.Max(lowestX.X, highestY.X);
            int yLoopMin = Math.Min(lowestX.Y, highestY.Y);

            // Increase ranges so we print more of the empty map

            for (int i = lowestX.X - 2; i <= xLoopMax + 2; i++)
            {
                for (int j = yLoopMin - 2; j <= highestY.Y + 2; j++)
                {
                    sb.Append((char)Map[i][j].TileType);
                }
                sb.AppendLine("");
            }

            return sb.ToString();
        }
    }
}
