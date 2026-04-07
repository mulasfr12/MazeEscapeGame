using System;

namespace MazeEscapeGame.Models
{
    public struct Position
    {
        public int X;
        public int Y;

        public Position(int x, int y) { X = x; Y = y; }

        public static Position operator +(Position a, Position b) => new(a.X + b.X, a.Y + b.Y);

        public static bool operator ==(Position a, Position b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Position a, Position b) => !(a == b);

        public override bool Equals(object obj) => obj is Position p && this == p;
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }
}
