namespace PipeHow.DungeonGenerator.Models
{
    public enum TileType
    {
        // Different whitespace between empty and floor needed
        Empty,
        Floor,
        Wall,
        WallVertical,
        WallHorizontal,
        WallCross,
        WallVerticalSeparatorLeft,
        WallVerticalSeparatorRight,
        WallHorizontalSeparatorUp,
        WallHorizontalSeparatorDown,
        WallCornerUpperRight,
        WallCornerUpperLeft,
        WallCornerLowerRight,
        WallCornerLowerLeft,
        WallCornerInnerUpperRight,
        WallCornerInnerUpperLeft,
        WallCornerInnerLowerRight,
        WallCornerInnerLowerLeft,
        Door
    }

    public interface ITile
    {
        int RoomId { get; set; }
        int Y { get; set; }
        int X { get; set; }
        TileType TileType { get; set; }
        TileType ShouldBeType { get; set; }
        string Symbol { get; }

        Direction Direction { get; set; }

        bool ShouldBeCorner();
    }
    public class Tile : ITile
    {
        public int RoomId { get; set; }
        public int Y { get; set; }
        public int X { get; set; }
        public Direction Direction { get; set; }

        private TileType tileType;
        /// <summary>
        /// The type of the tile. Setting this also sets the ShouldBeType.
        /// </summary>
        public TileType TileType { get => tileType; set { tileType = value; ShouldBeType = value; } }
        /// <summary>
        /// A tiletype value for evaluating changes before applying. Is also set by setting TileType.
        /// </summary>
        public TileType ShouldBeType { get; set; }
        public string Symbol {
            get {
                switch (TileType)
                {
                    case TileType.Empty:
                        return "X";
                    case TileType.Floor:
                        return " ";
                    case TileType.Wall:
                        return "W";
                    case TileType.WallVertical:
                        return "│";
                    case TileType.WallHorizontal:
                        return "─";
                    case TileType.WallCross:
                        return "┼";
                    case TileType.WallVerticalSeparatorLeft:
                        return "┤";
                    case TileType.WallVerticalSeparatorRight:
                        return "├";
                    case TileType.WallHorizontalSeparatorUp:
                        return "┴";
                    case TileType.WallHorizontalSeparatorDown:
                        return "┬";
                    case TileType.WallCornerUpperRight:
                        return "┐";
                    case TileType.WallCornerUpperLeft:
                        return "┌";
                    case TileType.WallCornerLowerRight:
                        return "┘";
                    case TileType.WallCornerLowerLeft:
                        return "└";
                    case TileType.WallCornerInnerUpperRight:
                        return "┐";
                    case TileType.WallCornerInnerUpperLeft:
                        return "┌";
                    case TileType.WallCornerInnerLowerRight:
                        return "┘";
                    case TileType.WallCornerInnerLowerLeft:
                        return "└";
                    case TileType.Door:
                        return "D";
                    default:
                        return "";
                }
            }
        }

        public Tile()
        {
            TileType = TileType.Empty;
            ShouldBeType = TileType.Empty;
        }

        public bool ShouldBeCorner() => ShouldBeType.ToString().Contains("Corner");

        public override string ToString() => $"{TileType}[{X}][{Y}]";
    }
}
