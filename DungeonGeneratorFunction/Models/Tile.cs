using System;
using System.Collections.Generic;
using System.Text;

namespace PipeHow.DungeonGenerator.Models
{
    public enum TileType
    {
        // Different whitespace between empty and floor needed
        Empty = 'X',
        Floor = ' ',
        Wall = 'W',
        WallVertical = '│',
        WallHorizontal = '─',
        WallCross = '┼',
        WallVerticalSeparatorLeft = '┤',
        WallVerticalSeparatorRight = '├',
        WallHorizontalSeparatorUp = '┴',
        WallHorizontalSeparatorDown = '┬',
        WallCornerUpperRight = '┐',
        WallCornerUpperLeft = '┌',
        WallCornerLowerRight = '┘',
        WallCornerLowerLeft = '└',
        Door = 'D'
    }

    public interface ITile
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

        public override string ToString() => $"{TileType}[{X}][{Y}]";
    }
}
