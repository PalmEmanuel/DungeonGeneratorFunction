using System;
using System.Collections.Generic;
using System.Text;

namespace PipeHow.DungeonGenerator.Models
{
    public enum TileType
    {
        // Different whitespace between empty and floor needed
        Empty = '　',
        Floor = ' ',
        Wall = 'W',
        WallVertical = '│',
        WallHorizontal = '─',
        WallCorner = '┼',
        WallCornerUpperRight = '┐',
        WallCornerUpperLeft = '┌',
        WallCornerLowerRight = '┘',
        WallCornerLowerLeft = '└',
        Door = 'D'
    }

    interface ITile
    {
        int Y { get; set; }
        int X { get; set; }
        TileType TileType { get; set; }
    }
    public class Tile : ITile
    {
        public int Y { get; set; }
        public int X { get; set; }
        public TileType TileType { get; set; }
        
        public Tile()
        {
            TileType = TileType.Empty;
        }
    }
}
