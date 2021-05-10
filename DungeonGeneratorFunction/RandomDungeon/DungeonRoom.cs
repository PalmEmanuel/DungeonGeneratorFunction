using System;

namespace PipeHow.DungeonMastery.RandomDungeon
{
    public enum RoomType
    {
        Room,
        Corridor
    }

    public interface IDungeonRoom
    {
        int Id { get; set; }
        RoomType RoomType { get; set; }
        ITile TopLeft { get; set; }
        ITile BottomRight { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        bool IsRoomCorner(ITile tile);
        bool IsRoomWall(ITile tile);
        bool IsInRoom(ITile tile);
        /// <summary>
        /// Gets the position of a random, non-corner, wall tile in the room.
        /// </summary>
        Tuple<int, int> GetRandomWallPosition(Random random);
    }

    public class DungeonRoom : IDungeonRoom
    {
        public int Id { get; set; }
        public RoomType RoomType { get; set; }
        public ITile TopLeft { get; set; }
        public ITile BottomRight { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public bool IsRoomCorner(ITile tile)
        {
            // Check if tile is on corner of room
            return tile.X == TopLeft.X && tile.Y == TopLeft.Y
                || tile.X == TopLeft.X && tile.Y == BottomRight.Y
                || tile.X == BottomRight.X && tile.Y == TopLeft.Y
                || tile.X == BottomRight.X && tile.Y == BottomRight.Y;
        }

        public bool IsRoomWall(ITile tile)
        {
            // Check if tile is in wall of room
            return IsInRoom(tile) && ((tile.X == TopLeft.X && (tile.Y >= TopLeft.Y || tile.Y <= BottomRight.Y)) ||
                    (tile.X == BottomRight.X && (tile.Y >= TopLeft.Y || tile.Y <= BottomRight.Y)) ||
                    (tile.Y == TopLeft.Y && (tile.X >= TopLeft.X || tile.X <= BottomRight.X)) ||
                    (tile.Y == BottomRight.Y && (tile.X >= TopLeft.X || tile.X <= BottomRight.X)));
        }

        public bool IsInRoom(ITile tile)
        {
            // Check if tile is within walls of room
            return tile.X >= TopLeft.X
                && tile.X <= BottomRight.X
                && tile.Y >= TopLeft.Y
                && tile.Y <= BottomRight.Y;
        }

        public Tuple<int,int> GetRandomWallPosition(Random random)
        {
            // Select which axis to randomize
            string randomAxis = new[] { "X", "Y" }[random.Next(0, 2)];

            // Create and initialize output variables
            int x, y;
            x = y = 0;
            switch (randomAxis)
            {
                case "X":
                    // Randomize top or bottom
                    y = new[] { TopLeft.Y, BottomRight.Y }[random.Next(0, 2)];

                    // Exclude corners
                    x = random.Next(TopLeft.X + 1, BottomRight.X);
                    break;
                case "Y":
                    // Randomize left or right
                    x = new[] { TopLeft.X, BottomRight.X }[random.Next(0, 2)];

                    y = random.Next(TopLeft.Y + 1, BottomRight.Y);
                    break;
            }

            return new Tuple<int, int>(x, y);
        }

        public override string ToString() => $"{Id}[{TopLeft.X},{TopLeft.Y}>{BottomRight.X},{BottomRight.Y}]";
    }
}