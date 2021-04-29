using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipeHow.DungeonGenerator.Models
{
    public interface IDungeon
    {
        int sizeX { get; set; }
        int sizeY { get; set; }
        /// <summary>
        /// The map is from top left to bottom right, first index is x, second is y.
        /// </summary>
        List<List<ITile>> Map { get; set; }
    }

    public class Dungeon : IDungeon
    {
        public int sizeX { get; set; }
        public int sizeY { get; set; }

        public List<List<ITile>> Map { get; set; }

        public Dungeon ()
        {
            Map = new List<List<ITile>>();
        }

        // Default max size of message in discord is 2000 characters
        // Square root of 2000 is just below 45
        // It's set to 43x43 to allow for 1 whitespace
        public static IDungeon CreateDungeon() { return CreateDungeon(43, 43); }

        public static IDungeon CreateDungeon(int width, int height)
        {
            Dungeon dungeon = new Dungeon();

            int originX = width / 2;
            int originY = height / 2;

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
            IRoom firstRoom = dungeon.CreateRoom(originX, originY, 6, 6);

            Random rand = new Random();
            var numberOfRooms = rand.Next(6, 12);
            List<IRoom> rooms = new List<IRoom>();
            for (int i = 0; i < numberOfRooms; i++)
            {
                int xPos = rand.Next(-originX/2 + 5, originX/2 - 5) + originX;
                int yPos = rand.Next(-originY/2 + 5, originY/2 - 5) + originY;
                int xSize = rand.Next(3, originX / 4);
                int ySize = rand.Next(3, originY / 4);
                rooms.Add(dungeon.CreateRoom(xPos, yPos, xSize, ySize));
                //dungeon.Map[xPos + xSize / 2][yPos + ySize / 2].TileType = TileType.Door;
            }

            dungeon.AddWalls();

            return dungeon;
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
                        bool emptyTopRight = IsEmpty(RightOf(Above(i, j)));
                        bool emptyTopLeft = IsEmpty(LeftOf(Above(i, j)));
                        bool emptyBottomRight = IsEmpty(RightOf(Below(i, j)));
                        bool emptyBottomLeft = IsEmpty(LeftOf(Below(i, j)));

                        // TODO: Switch case?

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
                        } // Vertical Wall or inwards-facing corner
                        else if (!emptyAbove && !emptyBelow)
                        {
                            // Left or right wall
                            if (emptyLeft != emptyRight)
                            {
                                Map[i][j].TileType = TileType.WallVertical;
                            }
                            else if (IsEmpty(LeftOf(Below(i, j))))
                            {
                                Map[i][j].TileType = TileType.WallCornerUpperRight;
                            }
                            else if (IsEmpty(RightOf(Below(i, j))))
                            {
                                Map[i][j].TileType = TileType.WallCornerUpperLeft;
                            }
                            else if (IsEmpty(LeftOf(Above(i, j))))
                            {
                                Map[i][j].TileType = TileType.WallCornerLowerRight;
                            }
                            else if (IsEmpty(RightOf(Above(i, j))))
                            {
                                Map[i][j].TileType = TileType.WallCornerLowerLeft;
                            }
                        }
                    }
                }
            }
        }

        private IRoom CreateRoom(int x, int y, int width, int height)
        {
            // Make sure x and y don't go outside the map
            if (x < 0 || y < 0) throw new ArgumentException("Ensure that x and y are not negative!");
            // If x or y plus requested rectangle size is outside map bounds, set to edge of map
            x = x + width > sizeY-1 ? width - sizeY : x;
            y = y + height > sizeX-1 ? height - sizeX : y;

            if (width < 3 || height < 3)
            {
                throw new ArgumentException("Rooms should be at least 3 wide to allow for edge floor to be converted to walls!");
            }
            FillRect(x, y, width, height, TileType.Floor);

            return new Room
            {
                BottomLeft = Map[x][y],
                TopRight = Map[x + width - 1][y + height - 1],
                Width = width,
                Height = height
            };
        }

        /// <summary>
        /// Sets a rectangle on the map to a specified TileType, starting from the bottom left corner of the position.
        /// </summary>
        private void FillRect(int x, int y, int width, int height, TileType type)
        {
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

            // Get corners of map
            int lowX = Map.SelectMany((row, i) => row.Select((value, j) => new { i, j, value }))
                .Where(v => v.value.TileType != TileType.Empty)
                .OrderBy(x => x.value.X).ThenBy(x => x.value.Y)
                .First().value.X;
            int highX = Map.SelectMany((row, i) => row.Select((value, j) => new { i, j, value }))
                .Where(v => v.value.TileType != TileType.Empty)
                .OrderByDescending(y => y.value.X).ThenByDescending(y => y.value.X)
                .First().value.X;
            int lowY = Map.SelectMany((row, i) => row.Select((value, j) => new { i, j, value }))
                .Where(v => v.value.TileType != TileType.Empty)
                .OrderBy(x => x.value.Y).ThenBy(x => x.value.Y)
                .First().value.Y;
            int highY = Map.SelectMany((row, i) => row.Select((value, j) => new { i, j, value }))
                .Where(v => v.value.TileType != TileType.Empty)
                .OrderByDescending(y => y.value.Y).ThenByDescending(y => y.value.X)
                .First().value.Y;

            // Whitespace should be based on map size
            // Get width and height of actual rooms, and get average between them
            // Get value based on that average
            int mapWhitespace = 1;
            lowX -= mapWhitespace;
            lowY -= mapWhitespace;
            highX += mapWhitespace;
            highY += mapWhitespace;

            // Add number grid for debugging
            // Copies of x loop
            sb.Append("   ");
            for (int j = lowX; j <= highX; j++)
            {
                sb.Append(j);
            }
            sb.AppendLine("");
            sb.Append("   ");
            for (int j = lowX; j <= highX; j++)
            {
                sb.Append("─");
            }
            sb.AppendLine("");

            // When printing we need to loop through y then x
            for (int i = lowY; i <= highY; i++)
            {
                sb.Append($"{i}|");
                for (int j = lowX; j <= highX; j++)
                {
                    sb.Append((char)Map[j][i].TileType);
                }
                sb.AppendLine("");
            }

            return sb.ToString();
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
        private ITile LeftOf(ITile tile) => LeftOf(tile.X, tile.Y);
        private ITile LeftOf(int x, int y)
        {
            return Map[x - 1][y];
        }
        private ITile RightOf(ITile tile) => RightOf(tile.X, tile.Y);
        private ITile RightOf(int x, int y)
        {
            return Map[x + 1][y];
        }
        private ITile Above(ITile tile) => Above(tile.X, tile.Y);
        private ITile Above(int x, int y)
        {
            return Map[x][y - 1];
        }
        private ITile Below(ITile tile) => Below(tile.X, tile.Y);
        private ITile Below(int x, int y)
        {
            return Map[x][y + 1];
        }
    }
}
