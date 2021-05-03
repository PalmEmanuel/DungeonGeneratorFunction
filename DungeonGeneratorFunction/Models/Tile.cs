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
        WallCornerInnerUpperRight = '┓',
        WallCornerInnerUpperLeft = '┏',
        WallCornerInnerLowerRight = '┛',
        WallCornerInnerLowerLeft = '┗',
        //WallCornerUpperRightBold = '┓',
        //WallCornerUpperLeftBold = '┏',
        //WallCornerLowerRightBold = '┛',
        //WallCornerLowerLeftBold = '┗',
        Door = 'D'
    }

    public interface ITile
    {
        Direction Direction { get; set; }
        int Y { get; set; }
        int X { get; set; }
        TileType TileType { get; set; }
        TileType ShouldBeType { get; set; }
        int RoomId { get; set; }
    }
    public class Tile : ITile
    {
        public Direction Direction { get; set; }
        public int Y { get; set; }
        public int X { get; set; }
        private TileType tileType;
        /// <summary>
        /// The type of the tile. Setting this also sets the ShouldBeType.
        /// </summary>
        public TileType TileType { get => tileType; set { tileType = value; ShouldBeType = value; } }
        /// <summary>
        /// A tiletype value for evaluating changes. Is also set by setting TileType.
        /// </summary>
        public TileType ShouldBeType { get; set; }
        public int RoomId { get; set; }
        
        public Tile()
        {
            TileType = TileType.Empty;
            ShouldBeType = TileType.Empty;
        }

        public override string ToString() => $"{TileType}[{X}][{Y}]";
    }
}
