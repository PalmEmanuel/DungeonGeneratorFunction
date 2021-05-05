using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        private List<IRoom> Rooms { get; set; }

        public Dungeon ()
        {
            Map = new List<List<ITile>>();
            Rooms = new List<IRoom>();
        }

        public static IDungeon CreateDungeon(int width, int height, int roomCount)
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
            dungeon.CreateRoom(originX, originY, 6, 6, 1);

            Random rand = new Random();
            for (int i = 0; i < roomCount - 1; i++)
            {
                int xPos = rand.Next(-originX / 2 + 5, originX / 2 - 5) + originX;
                int yPos = rand.Next(-originY / 2 + 5, originY / 2 - 5) + originY;
                int xSize = rand.Next(3, originX / 4);
                int ySize = rand.Next(3, originY / 4);
                // Create room with id 2 and onwards
                dungeon.CreateRoom(xPos, yPos, xSize, ySize, i + 2);
                //dungeon.Map[xPos + xSize / 2][yPos + ySize / 2].TileType = TileType.Door;
                Console.WriteLine($"dungeon.CreateRoom({xPos}, {yPos}, {xSize}, {ySize},{i + 2});");
            }
            // After adding all rooms, make sure the non-walls are floor
            dungeon.FillCommonFloor();
            // Set walls based on tiles relative types and positions to each other
            dungeon.AddWalls();

            return dungeon;
        }

        private void FillCommonFloor()
        {
            // Find position of all tiles that are in more than one room, where the tile is not a wall in all of the rooms
            var floorTilePositions = Map.SelectMany(row => row).Select(t => new
            {
                x = t.X,
                y = t.Y,
                roomCount = Rooms.Count(r => r.IsInRoom(t)), // Amount of rooms where the tile is not a wall
                wallCount = Rooms.Count(r => r.IsRoomWall(t)) // Amount of rooms where the tile is a wall
            }).Where(t => t.wallCount < t.roomCount).ToList();

            // Set found tiles to floor, since they are not the "outer wall" of the combined rooms
            foreach (var floorPos in floorTilePositions) {
                Map[floorPos.x][floorPos.y].TileType = TileType.Floor;
            }
        }

        private void AddWalls()
        {
            bool changesMade;
            do
            {
                changesMade = false;
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
                            // Check ShouldBeType for left or above (already processed tiles in loop)
                            // Check TileType for right or below (not yet processed tiles in loop)
                            if (ShouldBeCrossCorner(tile))
                            {
                                tile.TileType = TileType.WallCross;
                            } // Top left
                            else if (AdjacentTilesOf(tile, TileType.Floor) != 8 && ShouldBeWall(Below(tile)) && ShouldBeWall(RightOf(tile)) && ShouldBeFloor(RightOf(Below(tile))) && !IsFloor(LeftOf(Above(tile))) && !IsFloor(Above(tile)) && !IsFloor(LeftOf(tile)) &&
                                LeftOf(tile).TileType != TileType.WallCornerLowerLeft &&
                                (emptyAbove || (tile.RoomId != Above(tile).RoomId && Above(tile).ShouldBeType == TileType.WallCornerLowerRight && ShouldBeWall(LeftOf(Above(tile))))) &&
                                (emptyLeft || (tile.RoomId != LeftOf(tile).RoomId && LeftOf(tile).ShouldBeType == TileType.WallCornerLowerRight && ShouldBeWall(LeftOf(Above(tile))))))
                            {
                                tile.TileType = TileType.WallCornerUpperLeft;
                            } // Top right
                            else if (AdjacentTilesOf(tile, TileType.Floor) != 8 && ShouldBeWall(Below(tile)) && IsWall(LeftOf(tile)) && IsFloor(LeftOf(Below(tile))) && !ShouldBeFloor(RightOf(Above(tile))) && !IsFloor(Above(tile)) && !ShouldBeFloor(RightOf(tile)) &&
                                RightOf(tile).TileType != TileType.WallCornerLowerRight &&
                                (emptyAbove || (tile.RoomId != Above(tile).RoomId && Above(tile).ShouldBeType == TileType.WallCornerLowerLeft && ShouldBeWall(RightOf(Above(tile))))) &&
                                (emptyRight || (tile.RoomId != RightOf(tile).RoomId && RightOf(tile).ShouldBeType == TileType.WallCornerLowerLeft && ShouldBeWall(RightOf(Above(tile))))))
                            {
                                tile.TileType = TileType.WallCornerUpperRight;
                            } // Bottom left
                            else if (AdjacentTilesOf(tile, TileType.Floor) != 8 && IsWall(Above(tile)) && ShouldBeWall(RightOf(tile)) && ShouldBeFloor(RightOf(Above(tile))) && !IsFloor(LeftOf(Below(tile))) && !ShouldBeFloor(Below(tile)) && !IsFloor(LeftOf(tile)) &&
                                LeftOf(tile).TileType != TileType.WallCornerUpperLeft &&
                                (emptyBelow || (tile.RoomId != Below(tile).RoomId && Below(tile).ShouldBeType == TileType.WallCornerUpperRight && ShouldBeWall(LeftOf(Below(tile))))) &&
                                (emptyLeft || (tile.RoomId != LeftOf(tile).RoomId && LeftOf(tile).ShouldBeType == TileType.WallCornerUpperRight && ShouldBeWall(LeftOf(Below(tile))))))
                            {
                                tile.TileType = TileType.WallCornerLowerLeft;
                            } // Bottom right
                            else if (AdjacentTilesOf(tile, TileType.Floor) != 8 && IsWall(Above(tile)) && IsWall(LeftOf(tile)) && IsFloor(LeftOf(Above(tile))) && !ShouldBeFloor(RightOf(Below(tile))) && !ShouldBeFloor(Below(tile)) && !ShouldBeFloor(RightOf(tile)) &&
                                RightOf(tile).TileType != TileType.WallCornerUpperRight &&
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
                            else
                            {
                                if (IsEmpty(LeftOf(Above(tile))) &&
                                    ShouldBeWall(Above(tile)) && ShouldBeWall(LeftOf(tile)) &&
                                    (IsFloor(Below(i, j)) || IsFloor(RightOf(i, j))))
                                {
                                    tile.TileType = TileType.WallCornerInnerLowerRight;
                                }
                                else if (IsEmpty(RightOf(Above(tile))) &&
                                    ShouldBeWall(Above(tile)) && ShouldBeWall(RightOf(tile)) &&
                                    (IsFloor(Below(i, j)) || IsFloor(LeftOf(i, j))))
                                {
                                    tile.TileType = TileType.WallCornerInnerLowerLeft;
                                }
                                else if (IsEmpty(LeftOf(Below(tile))) &&
                                    ShouldBeWall(Below(tile)) && ShouldBeWall(LeftOf(tile)) &&
                                    (IsFloor(Above(i, j)) || IsFloor(RightOf(i, j))))
                                {
                                    tile.TileType = TileType.WallCornerInnerUpperRight;
                                }
                                else if (IsEmpty(RightOf(Below(tile))) &&
                                    ShouldBeWall(Below(tile)) && ShouldBeWall(RightOf(tile)) &&
                                    (IsFloor(Above(i, j)) || IsFloor(LeftOf(i, j))))
                                {
                                    tile.TileType = TileType.WallCornerInnerUpperLeft;
                                }
                            }
                        }
                        if (type != tile.TileType)
                        {
                            changesMade = true;
                        }
                    }
                }
            } while (changesMade);
        }

        private void CreateRoom(int x, int y, int width, int height, int id)
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
                    ITile tile = Map[i][j];

                    // If the created room is on a new space, set id and corners
                    if (tile.RoomId == 0)
                    {
                        tile.RoomId = id;

                        // Set edges of new room to walls
                        if (i == x || i == x + width - 1 || j == y || j == y + height - 1)
                        {
                            tile.ShouldBeType = TileType.Wall;
                        }

                        // Outer upper left
                        if (i == x && j == y)
                        {
                            tile.ShouldBeType = TileType.WallCornerUpperLeft;
                        } // Outer lower left
                        else if (i == x && j == y + height - 1)
                        {
                            tile.ShouldBeType = TileType.WallCornerLowerLeft;
                        } // Outer upper right
                        else if (i == x + width - 1 && j == y)
                        {
                            tile.ShouldBeType = TileType.WallCornerUpperRight;
                        } // Outer lower right
                        else if (i == x + width - 1 && j == y + height - 1)
                        {
                            tile.ShouldBeType = TileType.WallCornerLowerRight;
                        }
                    } // Modify tile properties where the room created intersects with an existing room
                    else if (tile.RoomId != id && tile.RoomId != 0)
                    {
                        // Find the existing room that the new room intersects with
                        var existingRoom = Rooms.First(r => r.Id == tile.RoomId);

                        // If the tile is within the room, evaluate if inner corner
                        if (existingRoom.IsInRoom(tile))
                        {
                            if (existingRoom.IsRoomWall(tile))
                            {
                                tile.ShouldBeType = TileType.Wall;

                                // If corners
                                // Outer upper left
                                if (i == x && j == y &&
                                    i == existingRoom.TopLeft.X && j == existingRoom.TopLeft.Y)
                                {
                                    tile.ShouldBeType = TileType.WallCornerUpperLeft;
                                } // Outer lower left
                                else if (i == x && j == y + height - 1 &&
                                    i == existingRoom.TopLeft.X && j == existingRoom.BottomRight.Y)
                                {
                                    tile.ShouldBeType = TileType.WallCornerLowerLeft;
                                } // Outer upper right
                                else if (i == x + width - 1 && j == y &&
                                    i == existingRoom.BottomRight.X && j == existingRoom.TopLeft.Y)
                                {
                                    tile.ShouldBeType = TileType.WallCornerUpperRight;
                                } // Outer lower right
                                else if (i == x + width - 1 && j == y + height - 1 &&
                                    i == existingRoom.BottomRight.X && j == existingRoom.BottomRight.Y)
                                {
                                    tile.ShouldBeType = TileType.WallCornerLowerRight;
                                }
                            }
                            else
                            {
                                // Inner upper left
                                if (i == x && j == y)
                                {
                                    tile.ShouldBeType = TileType.WallCornerInnerUpperLeft;
                                } // Inner lower left
                                else if (i == x && j == y + height - 1)
                                {
                                    tile.ShouldBeType = TileType.WallCornerInnerLowerLeft;
                                } // Inner upper right
                                else if (i == x + width - 1 && j == y)
                                {
                                    tile.ShouldBeType = TileType.WallCornerInnerUpperRight;
                                } // Inner lower right
                                else if (i == x + width - 1 && j == y + height - 1)
                                {
                                    tile.ShouldBeType = TileType.WallCornerInnerLowerRight;
                                }
                            }
                        }
                    }
                }
            }

            Rooms.Add(new Room
            {
                Id = id,
                TopLeft = Map[x][y],
                BottomRight = Map[x + width - 1][y + height - 1],
                Width = width,
                Height = height
            });
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

            //// Add number grid for debugging
            //// Copies of x loop
            //sb.Append("   ");
            //for (int j = lowX; j <= highX; j++)
            //{
            //    sb.Append(j);
            //}
            //sb.AppendLine("");
            //sb.Append("   ");
            //for (int j = lowX; j <= highX; j++)
            //{
            //    sb.Append("─");
            //}
            //sb.AppendLine("");

            // When printing we need to loop through y then x
            for (int i = lowY; i <= highY; i++)
            {
                //sb.Append($"{i}|");
                for (int j = lowX; j <= highX; j++)
                {
                    sb.Append(Map[j][i].Symbol);
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

        private int AdjacentTilesOf(ITile tile, string fuzzyType)
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
            }.Count(t => t.TileType.ToString().Contains(fuzzyType));
        }

        private bool ShouldBeWall(int x, int y)
        {
            return ShouldBeWall(Map[x][y]);
        }
        private bool ShouldBeWall(ITile tile)
        {
            return tile.ShouldBeType.ToString().Contains("Wall");
        }

        private bool IsViableCorner(ITile tile)
        {
            if (AdjacentTilesOf(tile, TileType.Floor) == 8 ||
                (tile.ShouldBeCorner() && !Above(tile).ShouldBeCorner() && !Below(tile).ShouldBeCorner()) ||
                (tile.ShouldBeCorner() && !RightOf(tile).ShouldBeCorner() && !LeftOf(tile).ShouldBeCorner()))
            {
                return false;
            }

            switch (tile.ShouldBeType)
            {
                case TileType.WallCornerUpperRight:
                    return !ShouldBeFloor(Above(tile))
                        && !ShouldBeFloor(RightOf(tile))
                        && !ShouldBeFloor(RightOf(Above(tile)))
                        && ShouldBeFloor(LeftOf(Below(tile)));
                    break;
                case TileType.WallCornerUpperLeft:
                    return !ShouldBeFloor(Above(tile))
                        && !ShouldBeFloor(LeftOf(tile))
                        && !ShouldBeFloor(LeftOf(Above(tile)))
                        && ShouldBeFloor(RightOf(Below(tile)));
                    break;
                case TileType.WallCornerLowerRight:
                    return !ShouldBeFloor(Below(tile))
                        && !ShouldBeFloor(RightOf(tile))
                        && !ShouldBeFloor(RightOf(Below(tile)))
                        && ShouldBeFloor(LeftOf(Above(tile)));
                    break;
                case TileType.WallCornerLowerLeft:
                    return !ShouldBeFloor(Below(tile))
                        && !ShouldBeFloor(LeftOf(tile))
                        && !ShouldBeFloor(LeftOf(Below(tile)))
                        && ShouldBeFloor(RightOf(Above(tile)));
                    break;
                case TileType.WallCornerInnerUpperRight:
                    return !IsEmpty(Above(tile))
                        && !IsEmpty(RightOf(tile))
                        && !IsEmpty(RightOf(Above(tile)))
                        && IsEmpty(LeftOf(Below(tile)));
                    break;
                case TileType.WallCornerInnerUpperLeft:
                    return !IsEmpty(Above(tile))
                        && !IsEmpty(LeftOf(tile))
                        && !IsEmpty(LeftOf(Above(tile)))
                        && IsEmpty(RightOf(Below(tile)));
                    break;
                case TileType.WallCornerInnerLowerRight:
                    return !IsEmpty(Below(tile))
                        && !IsEmpty(RightOf(tile))
                        && !IsEmpty(RightOf(Below(tile)))
                        && IsEmpty(LeftOf(Above(tile)));
                    break;
                case TileType.WallCornerInnerLowerLeft:
                    return !IsEmpty(Below(tile))
                        && !IsEmpty(LeftOf(tile))
                        && !IsEmpty(LeftOf(Below(tile)))
                        && IsEmpty(RightOf(Above(tile)));
                    break;
            }

            return false;
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
                && (Regex.IsMatch(Above(x, y).ShouldBeType.ToString(), "WallVertical") || Regex.IsMatch(Above(x, y).ShouldBeType.ToString(), "WallCornerLower(Left|Right)"))
                && (Regex.IsMatch(Below(x, y).ShouldBeType.ToString(), "WallVertical") || Regex.IsMatch(Below(x, y).ShouldBeType.ToString(), "WallCornerUpper(Left|Right)"))
                && (Regex.IsMatch(LeftOf(x, y).ShouldBeType.ToString(), "WallHorizontal") || Regex.IsMatch(LeftOf(x, y).ShouldBeType.ToString(), "WallCorner(Lower|Upper)Right"))
                && (Regex.IsMatch(RightOf(x, y).ShouldBeType.ToString(), "WallHorizontal") || Regex.IsMatch(RightOf(x, y).ShouldBeType.ToString(), "WallCorner(Lower|Upper)Left"));
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
