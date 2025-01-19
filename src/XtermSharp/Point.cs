namespace XtermSharp
{
    public struct Point
    {
        public static readonly Point Empty;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public bool IsEmpty => X == 0 && Y == 0;

        public static bool operator ==(Point left, Point right) => left.X == right.X && left.Y == right.Y;

        public static bool operator !=(Point left, Point right) => !(left == right);

        public override bool Equals(object obj) => obj is Point point && point.X == X && point.Y == Y;

        public override int GetHashCode() => X.GetHashCode() * 327 + Y.GetHashCode();

        public override string ToString() => "{X=" + X.ToString() + ",Y=" + Y.ToString() + "}";
    }
}
