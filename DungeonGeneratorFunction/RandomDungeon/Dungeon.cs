using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PipeHow.DungeonMastery.RandomDungeon
{
    public interface IDungeon
    {
        int Width { get; set; }
        int Height { get; set; }
        /// <summary>
        /// The map is from top left to bottom right, first index is x, second is y.
        /// </summary>
        List<List<ITile>> Map { get; set; }
        List<IDungeonRoom> Rooms { get; set; }
        
        /// <summary>
        /// The random generator used for all randomization in the dungeon.
        /// </summary>
        Random Random { get; set; }
        /// <summary>
        /// The seed used for the specific dungeon.
        /// </summary>
        int Seed { get; set; }

        public ITile RightOf(ITile tile);
        public ITile LeftOf(ITile tile);
        public ITile Above(ITile tile);
        public ITile Below(ITile tile);
        public bool IsCorner(ITile tile);
    }

    public class Dungeon : IDungeon
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public List<List<ITile>> Map { get; set; }

        public List<IDungeonRoom> Rooms { get; set; }

        public Random Random { get; set; }
        public int Seed { get; set; }

        public Dungeon ()
        {
            Map = new List<List<ITile>>();
            Rooms = new List<IDungeonRoom>();
        }

        public static IDungeon CreateDungeon(int width, int height, int roomMinSize = 4, int roomMaxSize = 8, int roomCount = 7, int seed = 0)
        {
            Dungeon dungeon = new Dungeon();

            // If seed not provided, create seed
            dungeon.Seed = seed == 0 ? Environment.TickCount : seed;
            dungeon.Random = new Random(dungeon.Seed);

            Console.WriteLine($"Seed: {dungeon.Seed}");

            int originX = width / 2;
            int originY = height / 2;

            dungeon.Height = width;
            dungeon.Width = height;

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
            int roomWidth = dungeon.Random.Next(roomMinSize, roomMaxSize);
            int roomHeight = dungeon.Random.Next(roomMinSize, roomMaxSize);
            dungeon.CreateRoom(originX, originY, roomWidth, roomHeight, 1);

            // TODO: Evaluate logic where all rooms are placed first and corridors are placed between them
            // TODO: Also try with forcing a different direction from the last corridor
            for (int i = 0; i < roomCount-1; i++)
            {
                bool createdCorridor = false;
                ITile wallTile;
                Tuple<int, int> wallPos;
                while (!createdCorridor)
                {
                    // Get a random wall tile from the first room
                    wallPos = dungeon.Rooms.Last().GetRandomWallPosition(dungeon.Random);
                    wallTile = dungeon.Map[wallPos.Item1][wallPos.Item2];

                    // Create a corridor to the next room
                    int corridorWidth = dungeon.Random.Next(3, 5);
                    int corridorLength = dungeon.Random.Next(6, 12);
                    createdCorridor = dungeon.CreateCorridor(wallTile, corridorWidth, corridorLength, dungeon.Rooms.Count() + 1);
                }

                wallPos = dungeon.Rooms.Last().GetRandomWallPosition(dungeon.Random);
                wallTile = dungeon.Map[wallPos.Item1][wallPos.Item2];
                roomWidth = dungeon.Random.Next(roomMinSize, roomMaxSize);
                roomHeight = dungeon.Random.Next(roomMinSize, roomMaxSize);
                dungeon.CreateRoom(wallTile.X, wallTile.Y, roomWidth, roomHeight, dungeon.Rooms.Count() + 1);
            }

            //for (int i = 0; i < roomCount - 1; i++)
            //{
            //    int xPos = dungeon.Random.Next(-originX / 2 + 5, originX / 2 - 5) + originX;
            //    int yPos = dungeon.Random.Next(-originY / 2 + 5, originY / 2 - 5) + originY;
            //    int xSize = dungeon.Random.Next(3, originX / 4);
            //    int ySize = dungeon.Random.Next(3, originY / 4);
            //    // Create room with id 2 and onwards
            //    dungeon.CreateRoom(xPos, yPos, xSize, ySize, i + 2);
            //    //dungeon.Map[xPos + xSize / 2][yPos + ySize / 2].TileType = TileType.Door;
            //    Console.WriteLine($"dungeon.CreateRoom({xPos}, {yPos}, {xSize}, {ySize},{i + 2});");
            //}
            // After adding all rooms, make sure the non-walls in intersecting rooms are floor
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

        private bool CreateCorridor(ITile tile, int width, int length, int id)
        {
            // TODO: Set width or length to edge instead?
            if (tile.X + length > Width - 1 ||
                tile.Y + length > Height - 1 ||
                tile.X - length < 2 ||
                tile.Y - length < 2)
            {
                return false;
            }
            var direction = tile.Direction;
            ITile topLeft, bottomRight;
            // Offset by half of the width, plus 1 if uneven
            int offset = (width/2) + ((width / 2) % 2);
            // Decide which direction to place corridor
            switch (direction)
            {
                case Direction.WEST:
                    topLeft = Map[tile.X - length + 1][tile.Y - offset];
                    bottomRight = Map[tile.X + 1][tile.Y + width - offset];
                    break;
                case Direction.EAST:
                    topLeft = Map[tile.X - 1][tile.Y - offset];
                    bottomRight = Map[tile.X + length - 1][tile.Y + width - offset];
                    break;
                case Direction.NORTH:
                    topLeft = Map[tile.X - width + offset][tile.Y - length + 1];
                    bottomRight = Map[tile.X + offset][tile.Y + 1];
                    break;
                case Direction.SOUTH:
                    topLeft = Map[tile.X - offset][tile.Y - 1];
                    bottomRight = Map[tile.X + width - offset][tile.Y + length - 1];
                    break;
                default:
                    throw new ArgumentException("Unknown direction!");
            }

            // TODO: Decide placement logic in IsViableCorridor. Only separate rooms, or allow intersection?
            if (!IsViableCorridor(topLeft, bottomRight))
            {
                return false;
            }
            CreateRoom(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y, id);

            Rooms.Last().RoomType = RoomType.Corridor;

            return true;
        }

        private void CreateRoom(int x, int y, int width, int height, int id)
        {
            // Make sure x and y don't go outside the map
            if (x < 2 || y < 2) throw new ArgumentException("Ensure that x and y are not negative!");
            // If x or y plus requested rectangle size is outside map bounds, set to edge of map
            x = x + width > Height - 1 ? Height - width - 2: x;
            y = y + height > Width - 1 ? Width - height - 2: y;

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

                    // Calculate tile Direction from room center by measuring whether the x or y is further from the center, relative to room size
                    int roomCenterX = x + (width / 2);
                    int roomCenterY = y + (height / 2);
                    float distancePercentageUnitX = (float)width / 2 / 100;
                    float distancePercentageUnitY = (float)height / 2 / 100;

                    // Get distance from center in percentage of room size
                    int distanceX = (int)(Math.Abs(i - roomCenterX) / distancePercentageUnitX);
                    int distanceY = (int)(Math.Abs(i - roomCenterY) / distancePercentageUnitY);
                    // If the tile is horizontally further away from the center than vertically, relative to room size
                    if (distanceX > distanceY)
                    {
                        // West if the tile is left of or at the room center, otherwise east
                        // This means there's a slight bias for the middle tile
                        tile.Direction = i <= roomCenterX ? Direction.WEST : Direction.EAST;

                    } // Vertically further away than horizontally
                    else if (distanceY > distanceX)
                    {
                        tile.Direction = j <= roomCenterY ? Direction.NORTH : Direction.SOUTH;
                    }
                    else
                    {
                        // If the tile is equally far away from the center horizontally and vertically, pick random
                        if (i <= roomCenterX && j <= roomCenterY)
                        {
                            tile.Direction = RandomDirection(Direction.WEST, Direction.NORTH);
                        }
                        else if (i <= roomCenterX && j > roomCenterY)
                        {
                            tile.Direction = RandomDirection(Direction.WEST, Direction.SOUTH);
                        }
                        else if (i > roomCenterX && j <= roomCenterY)
                        {
                            tile.Direction = RandomDirection(Direction.EAST, Direction.NORTH);
                        }
                        else if (i > roomCenterX && j > roomCenterY)
                        {
                            tile.Direction = RandomDirection(Direction.EAST, Direction.SOUTH);
                        }
                    }

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

            Rooms.Add(new DungeonRoom
            {
                RoomType = RoomType.Room,
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

            // Stamp map with seed
            string seedString = $"{Seed}";
            return string.Join("", sb.ToString().Substring(0, sb.Length - seedString.Length - 2), seedString);
        }

        private bool IsViableCorridor(ITile topLeft, ITile bottomRight)
        {
            if (bottomRight.X > Width - 1 ||
                bottomRight.Y > Height - 1 ||
                topLeft.X < 2 ||
                topLeft.Y < 2)
            {
                return false;
            }
            //for (int i = topLeft.X; i < bottomRight.X; i++)
            //{
            //    for (int j = topLeft.Y; j < bottomRight.Y; j++)
            //    {
            //        if (IsEmpty(Map[i][j]))
            //        {
            //            return false;
            //        }
            //    }
            //}

            return true;
        }

        internal bool IsEmpty(int x, int y)
        {
            return IsEmpty(Map[x][y]);
        }
        internal bool IsEmpty(ITile tile)
        {
            return tile.TileType == TileType.Empty;
        }
        internal bool IsFloor(int x, int y)
        {
            return IsFloor(Map[x][y]);
        }
        internal bool IsFloor(ITile tile)
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
        internal bool IsWall(int x, int y)
        {
            return IsWall(Map[x][y]);
        }

        internal int AdjacentTilesOf(ITile tile, TileType type)
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

        public bool IsCorner(ITile tile)
        {
            return tile.ShouldBeType.ToString().Contains("Corner");
        }
        private bool IsCorner(int x, int y)
        {
            return IsCorner(Map[x][y]);
        }
        private bool ShouldBeWall(int x, int y)
        {
            return ShouldBeWall(Map[x][y]);
        }
        private bool ShouldBeWall(ITile tile)
        {
            return tile.ShouldBeType.ToString().Contains("Wall");
        }

        internal bool IsWall(ITile tile)
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
        public ITile LeftOf(ITile tile) => LeftOf(tile.X, tile.Y);
        private ITile LeftOf(int x, int y)
        {
            return Map[x - 1][y];
        }
        public ITile RightOf(ITile tile) => RightOf(tile.X, tile.Y);
        private ITile RightOf(int x, int y)
        {
            return Map[x + 1][y];
        }
        public ITile Above(ITile tile) => Above(tile.X, tile.Y);
        private ITile Above(int x, int y)
        {
            return Map[x][y - 1];
        }
        public ITile Below(ITile tile) => Below(tile.X, tile.Y);
        private ITile Below(int x, int y)
        {
            return Map[x][y + 1];
        }

        private Direction RandomDirection(params Direction[] directions)
        {
            return directions[Random.Next(directions.Count())];
        }
    }
}
