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
            IRoom firstRoom = dungeon.CreateRoom(originX, originY, 6, 6, 1);

            Random rand = new Random();
            var numberOfRooms = rand.Next(6, 12);
            List<IRoom> rooms = new List<IRoom>();
            for (int i = 0; i < numberOfRooms; i++)
            {
                int xPos = rand.Next(-originX / 2 + 5, originX / 2 - 5) + originX;
                int yPos = rand.Next(-originY / 2 + 5, originY / 2 - 5) + originY;
                int xSize = rand.Next(3, originX / 4);
                int ySize = rand.Next(3, originY / 4);
                // Create room with id 2 and onwards
                rooms.Add(dungeon.CreateRoom(xPos, yPos, xSize, ySize, i + 2));
                //dungeon.Map[xPos + xSize / 2][yPos + ySize / 2].TileType = TileType.Door;
            }

            //rooms.Add(dungeon.CreateRoom(18, 17, 3, 3, 2));
            //rooms.Add(dungeon.CreateRoom(20, 19, 3, 8, 3));

            while (dungeon.AddWalls());

            return dungeon;
        }

        private bool AddWalls()
        {
            bool changesMade = false;
            for (int i = 0; i < Map.Count; i++)
            {
                for (int j = 0; j < Map[i].Count; j++)
                {
                    var tile = Map[i][j];
                    TileType type = tile.TileType;

                    // Go through floor tiles
                    if (IsFloor(tile) || IsWall(tile))
                    {
                        bool emptyAbove = IsEmpty(Above(tile));
                        bool emptyBelow = IsEmpty(Below(tile));
                        bool emptyLeft = IsEmpty(LeftOf(tile));
                        bool emptyRight = IsEmpty(RightOf(tile));
                        
                        // Is wall and has walls on each side
                        if (ShouldBeCrossCorner(tile))
                        {
                            tile.TileType = TileType.WallCross;
                        } // Top left
                        else if (ShouldBeWall(Below(tile)) && ShouldBeWall(RightOf(tile)) && IsFloor(RightOf(Below(tile))) && !ShouldBeFloor(LeftOf(Above(tile))) && !ShouldBeFloor(LeftOf(tile)) &&
                            !Above(tile).ShouldBeType.ToString().Contains("Upper") &&
                            (emptyAbove || (tile.RoomId != Above(tile).RoomId && Above(tile).ShouldBeType == TileType.WallCornerLowerRight && ShouldBeWall(LeftOf(Above(tile))))) &&
                            (emptyLeft || (tile.RoomId != LeftOf(tile).RoomId && LeftOf(tile).ShouldBeType == TileType.WallCornerLowerRight && ShouldBeWall(LeftOf(Above(tile))))))
                        {
                            tile.TileType = TileType.WallCornerUpperLeft;
                        } // Top right
                        else if (ShouldBeWall(Below(tile)) && ShouldBeWall(LeftOf(tile)) && IsFloor(LeftOf(Below(tile))) && !ShouldBeFloor(RightOf(Above(tile))) && !ShouldBeFloor(RightOf(tile)) &&
                            !Above(tile).ShouldBeType.ToString().Contains("Upper") &&
                            (emptyAbove || (tile.RoomId != Above(tile).RoomId && Above(tile).ShouldBeType == TileType.WallCornerLowerLeft && ShouldBeWall(RightOf(Above(tile))))) &&
                            (emptyRight || (tile.RoomId != RightOf(tile).RoomId && RightOf(tile).ShouldBeType == TileType.WallCornerLowerLeft && ShouldBeWall(RightOf(Above(tile))))))
                        {
                            tile.TileType = TileType.WallCornerUpperRight;
                        } // Bottom left
                        else if (ShouldBeWall(Above(tile)) && ShouldBeWall(RightOf(tile)) && IsFloor(RightOf(Above(tile))) && !ShouldBeFloor(LeftOf(Below(tile))) && !ShouldBeFloor(LeftOf(tile)) &&
                            !Below(tile).ShouldBeType.ToString().Contains("Lower") &&
                            (emptyBelow || (tile.RoomId != Below(tile).RoomId && Below(tile).ShouldBeType == TileType.WallCornerUpperRight && ShouldBeWall(LeftOf(Below(tile))))) &&
                            (emptyLeft || (tile.RoomId != LeftOf(tile).RoomId && LeftOf(tile).ShouldBeType == TileType.WallCornerUpperRight && ShouldBeWall(LeftOf(Below(tile))))))
                        {
                            tile.TileType = TileType.WallCornerLowerLeft;
                        } // Bottom right
                        else if (ShouldBeWall(Above(tile)) && ShouldBeWall(LeftOf(tile)) && IsFloor(LeftOf(Above(tile))) && !ShouldBeFloor(RightOf(Below(tile))) && !ShouldBeFloor(RightOf(tile)) &&
                            !Below(tile).ShouldBeType.ToString().Contains("Lower") &&
                            (emptyBelow || (tile.RoomId != Below(tile).RoomId && Below(tile).ShouldBeType == TileType.WallCornerUpperLeft && ShouldBeWall(RightOf(Below(tile))))) &&
                            (emptyRight || (tile.RoomId != RightOf(tile).RoomId && RightOf(tile).ShouldBeType == TileType.WallCornerUpperLeft && ShouldBeWall(RightOf(Below(tile))))))
                        {
                            tile.TileType = TileType.WallCornerLowerRight;
                        } // Horizontal wall
                        else if (IsEmpty(Above(tile)) && !IsEmpty(Below(tile)) || !IsEmpty(Above(tile)) && IsEmpty(Below(tile)) && !tile.TileType.ToString().Contains("Corner"))
                        {
                            tile.TileType = TileType.WallHorizontal;
                        } // Vertical Wall
                        else if (IsEmpty(LeftOf(tile)) && !IsEmpty(RightOf(tile)) || !IsEmpty(LeftOf(tile)) && IsEmpty(RightOf(tile)) && !tile.TileType.ToString().Contains("Corner"))
                        {
                            tile.TileType = TileType.WallVertical;
                        } // Inwards-facing corners
                        else if (!IsWall(tile))
                        {
                            if (IsEmpty(LeftOf(Above(tile))) &&
                                IsWall(Above(tile)) && IsWall(LeftOf(tile)) &&
                                (IsFloor(Below(i, j)) || IsFloor(RightOf(i, j))))
                            {
                                tile.TileType = TileType.WallCornerLowerRight;
                            }
                            else if (IsEmpty(RightOf(Above(tile))) &&
                                IsWall(Above(tile)) && IsWall(RightOf(tile)) &&
                                (IsFloor(Below(i, j)) || IsFloor(LeftOf(i, j))))
                            {
                                tile.TileType = TileType.WallCornerLowerLeft;
                            }
                            else if (IsEmpty(LeftOf(Below(tile))) &&
                                IsWall(Below(tile)) && IsWall(LeftOf(tile)) &&
                                (IsFloor(Above(i, j)) || IsFloor(RightOf(i, j))))
                            {
                                tile.TileType = TileType.WallCornerUpperRight;
                            }
                            else if (IsEmpty(RightOf(Below(tile))) &&
                                IsWall(Below(tile)) && IsWall(RightOf(tile)) &&
                                (IsFloor(Above(i, j)) || IsFloor(LeftOf(i, j))))
                            {
                                tile.TileType = TileType.WallCornerUpperLeft;
                            }
                        }
                    }
                    if (type != tile.TileType)
                    {
                        changesMade = true;
                    }
                }
            }
            return changesMade;
        }

        private IRoom CreateRoom(int x, int y, int width, int height, int id)
        {
            // Make sure x and y don't go outside the map
            if (x < 0 || y < 0) throw new ArgumentException("Ensure that x and y are not negative!");
            // If x or y plus requested rectangle size is outside map bounds, set to edge of map
            x = x + width > sizeY - 1 ? width - sizeY : x;
            y = y + height > sizeX - 1 ? height - sizeX : y;

            if (width < 3 || height < 3)
            {
                throw new ArgumentException("Rooms should be at least 3 wide to allow for edge floor to be converted to walls!");
            }
            FillRect(x, y, width, height, TileType.Floor);

            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    if ((i == x || i == x + width - 1) || (j == y || j == y + height - 1))
                    {
                        if (Map[i][j].RoomId == 0) {
                            Map[i][j].ShouldBeType = TileType.Wall;
                        }
                    }
                    Map[i][j].RoomId = id;
                }
            }

            Room room = new Room
            {
                Id = id,
                TopLeft = Map[x][y],
                BottomRight = Map[x + width - 1][y + height - 1],
                Width = width,
                Height = height
            };

            Map[x][y].ShouldBeType = TileType.WallCornerUpperLeft;
            Map[x + width - 1][y].ShouldBeType = TileType.WallCornerUpperRight;
            Map[x][y + height - 1].ShouldBeType = TileType.WallCornerLowerLeft;
            Map[x + width - 1][y + height - 1].ShouldBeType = TileType.WallCornerLowerRight;

            return room;
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

            // Padding for the map
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
        private bool ShouldBeFloor(int x, int y)
        {
            return ShouldBeFloor(Map[x][y]);
        }
        private bool ShouldBeFloor(ITile tile)
        {
            return tile.ShouldBeType == TileType.Floor;
        }
        private bool IsWall(int x, int y)
        {
            return IsWall(Map[x][y]);
        }

        private int AdjacentTilesOf(ITile tile, TileType type)
        {
            // Create a list with the 8 adjacent tiles and return the amount matching the type
            return new List<ITile>
            {
                Above(tile),
                Above(LeftOf(tile)),
                Above(RightOf(tile)),
                Below(tile),
                Below(LeftOf(tile)),
                Below(RightOf(tile)),
                LeftOf(tile),
                RightOf(tile),
            }.Count(t => t.TileType == type);
        }

        private bool ShouldBeWall(int x, int y)
        {
            return ShouldBeWall(Map[x][y]);
        }
        private bool ShouldBeWall(ITile tile)
        {
            return tile.ShouldBeType.ToString().Contains("Wall");
        }

        private bool IsWall(ITile tile)
        {
            return tile.TileType.ToString().Contains("Wall");
        }

        private bool ShouldBeCrossCorner(ITile tile)
        {
            return ShouldBeCrossCorner(tile.X, tile.Y);
        }
        private bool ShouldBeCrossCorner(int x, int y)
        {
            // If the tile is a wall or floor tile, and has walls with the right alignment directly beside it (non-diagonal)
            return (ShouldBeWall(x, y) || ShouldBeFloor(x, y))
                && Above(x, y).ShouldBeType.ToString().Contains(TileType.WallVertical.ToString())
                && Below(x, y).ShouldBeType.ToString().Contains(TileType.WallVertical.ToString())
                && LeftOf(x, y).ShouldBeType.ToString().Contains(TileType.WallHorizontal.ToString())
                && RightOf(x, y).ShouldBeType.ToString().Contains(TileType.WallHorizontal.ToString());
        }
        private bool IsCrossCorner(ITile tile)
        {
            return IsCrossCorner(tile.X, tile.Y);
        }
        private bool IsCrossCorner(int x, int y)
        {
            // If the tile is a wall or floor tile, and has walls with the right alignment directly beside it (non-diagonal)
            return (IsWall(x, y) || IsFloor(x, y))
                && Above(x, y).TileType.ToString().Contains(TileType.WallVertical.ToString())
                && Below(x, y).TileType.ToString().Contains(TileType.WallVertical.ToString())
                && LeftOf(x, y).TileType.ToString().Contains(TileType.WallHorizontal.ToString())
                && RightOf(x, y).TileType.ToString().Contains(TileType.WallHorizontal.ToString());
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
