using System;

namespace PipeHow.DungeonGenerator.Models
{
    interface IRoom
    {
        ITile TopRight { get; set; }
        ITile BottomLeft { get; set; }
        int Width { get; set; }
        int Height { get; set; }
    }
    internal class Room : IRoom
    {
        public ITile TopRight { get; set; }
        public ITile BottomLeft { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        internal bool IsRoomWall(ITile tile)
        {
            // Check if tile is on edge of room (in wall)
            return (tile.X == BottomLeft.X || tile.X < TopRight.X) && (tile.Y > BottomLeft.Y || tile.Y < TopRight.Y);
        }

        internal bool IsInRoom(ITile tile)
        {
            // Check if tile is within room
            return tile.X > BottomLeft.X
                && tile.X < TopRight.X
                && tile.Y > BottomLeft.Y
                && tile.Y < TopRight.Y;
        }

        //public ITile GetRandomWallTile()
        //{
            
        //}
    }
}