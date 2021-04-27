using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeHow.DungeonGenerator.Models
{
    interface IDungeon
    {
        int sizeX { get; set; }
        int sizeY { get; set; }
    }

    public class Dungeon : IDungeon
    {
        public int sizeX { get; set; }
        public int sizeY { get; set; }

        List<List<ITile>> Map { get; set; }

        public Dungeon ()
        {
            Map = new List<List<ITile>>();
        }

        internal static IDungeon CreateDungeon() { return CreateDungeon(100, 100); }

        internal static IDungeon CreateDungeon(int width, int height)
        {
            Dungeon dungeon = new Dungeon();

            dungeon.sizeY = width;
            dungeon.sizeX = height;

            // Initialize dungeon map tiles
            for (int i = 0; i < width; i++)
            {
                dungeon.Map.Add(new List<ITile>());
                for (int j = 0; j < height; j++)
                {
                    dungeon.Map[i].Add(new Tile { X = i, Y = j });
                }
            }

            // Create first room in center
            dungeon.CreateRoom(width/2, height/2, 6, 6);

            dungeon.AddWalls();

            return dungeon;
        }

        private bool IsEmpty(int x, int y)
        {
            return IsEmpty(Map[x][y]);
        }
        private bool IsEmpty(ITile tile)
        {
            return tile.TileType == TileType.Empty;
        }
        private bool IsFloor(int x, int y)
        {
            return IsFloor(Map[x][y]);
        }
        private bool IsFloor(ITile tile)
        {
            return tile.TileType == TileType.Floor;
        }
        private bool IsWall(int x, int y)
        {
            return IsWall(Map[x][y]);
        }

        private bool IsWall(ITile tile)
        {
            // If the type contains "Wall"
            return tile.TileType.ToString().Contains(TileType.Wall.ToString());
        }

        private bool IsUpperLeftCorner(int x, int y)
        {
            return IsWall(x, y)
                && !IsWall(Above(x, y))
                && IsWall(Below(x, y))
                && !IsWall(LeftOf(x, y))
                && IsWall(RightOf(x, y));
        }
        private bool IsUpperRightCorner(int x, int y)
        {
            return IsWall(x, y)
                && !IsWall(Above(x, y))
                && IsWall(Below(x, y))
                && IsWall(LeftOf(x, y))
                && !IsWall(RightOf(x, y));
        }
        private bool IsLowerRightCorner(int x, int y)
        {
            return IsWall(x, y)
                && IsWall(Above(x, y))
                && !IsWall(Below(x, y))
                && IsWall(LeftOf(x, y))
                && !IsWall(RightOf(x, y));
        }
        private bool IsLowerLeftCorner(int x, int y)
        {
            return IsWall(x, y)
                && IsWall(Above(x, y))
                && !IsWall(Below(x, y))
                && !IsWall(LeftOf(x, y))
                && IsWall(RightOf(x, y));
        }
        private ITile LeftOf(int x, int y)
        {
            return Map[x - 1][y];
        }
        private ITile RightOf(int x, int y)
        {
            return Map[x + 1][y];
        }
        private ITile Above(int x, int y)
        {
            return Map[x][y - 1];
        }
        private ITile Below(int x, int y)
        {
            return Map[x][y + 1];
        }

        private void AddWalls()
        {
            for (int i = 0; i < Map.Count; i++)
            {
                for (int j = 0; j < Map[i].Count; j++)
                {
                    var tile = Map[i][j];

                    // Go through floor tiles
                    if (IsFloor(tile))
                    {
                        bool emptyAbove = IsEmpty(Above(i, j));
                        bool emptyBelow = IsEmpty(Below(i, j));
                        bool emptyLeft = IsEmpty(LeftOf(i, j));
                        bool emptyRight = IsEmpty(RightOf(i, j));

                        // Top wall
                        if (emptyAbove && !emptyBelow)
                        {
                            // Top left
                            if (emptyLeft && !emptyRight)
                            {
                                Map[i][j].TileType = TileType.WallCornerUpperLeft;
                            } // Top right
                            else if (!emptyLeft && emptyRight)
                            {
                                Map[i][j].TileType = TileType.WallCornerUpperRight;
                            }
                            else
                            {
                                Map[i][j].TileType = TileType.WallHorizontal;
                            }
                        } // Bottom wall
                        else if (!emptyAbove && emptyBelow)
                        {
                            // Bottom left
                            if (emptyLeft && !emptyRight)
                            {
                                Map[i][j].TileType = TileType.WallCornerLowerLeft;
                            } // Bottom right
                            else if (!emptyLeft && emptyRight)
                            {
                                Map[i][j].TileType = TileType.WallCornerLowerRight;
                            }
                            else
                            {
                                Map[i][j].TileType = TileType.WallHorizontal;
                            }
                        } // Vertical Wall
                        else if (!emptyAbove && !emptyBelow)
                        {
                            // Left wall
                            if (emptyLeft && !emptyRight)
                            {
                                Map[i][j].TileType = TileType.WallVertical;
                            }

                            // Right wall
                            if (!emptyLeft && emptyRight)
                            {
                                Map[i][j].TileType = TileType.WallVertical;
                            }
                        }
                    }
                }
            }
        }

        private void CreateRoom(int x, int y, int width, int height)
        {
            if (width < 3 || height < 3)
            {
                throw new ArgumentException("Rooms should be at least 3 wide to allow for edge floor to be converted to walls!");
            }
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
            x = x + width > sizeY ? width - sizeY : x;
            y = y + height > sizeX ? height - sizeX : y;

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

            // Select corners of map and add whitespace for printing
            int lowX = lowestX.X;
            int lowY = Math.Min(lowestX.Y, highestY.Y);
            int highX = Math.Max(lowestX.X, highestY.X);
            int highY = highestY.Y;

            // Whitespace should be based on map size
            // Get width and height of actual rooms, and get average between them
            // Get value based on that average
            int mapWhitespace = (highX - lowX + highY - lowY) / 5;
            mapWhitespace = Math.Max(mapWhitespace, 6);
            mapWhitespace = Math.Min(mapWhitespace, 3);
            lowX -= mapWhitespace;
            lowY -= mapWhitespace;
            highX += mapWhitespace;
            highY += mapWhitespace;

            // When printing we need to loop through y then x
            for (int i = lowY; i <= highY; i++) 
            {
                for (int j = lowX; j <= highX; j++)
                {
                    sb.Append((char)Map[j][i].TileType);
                }
                sb.AppendLine("");
            }

            return sb.ToString();
        }
    }
}
