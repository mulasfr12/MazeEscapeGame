using System;
using System.Collections.Generic;
using MazeEscapeGame.Models;

namespace MazeEscapeGame.Core
{
    public class MazeGrid
    {
        private readonly TileType[,] _tiles;
        private readonly bool[,]     _revealed;

        public int Width  { get; }
        public int Height { get; }

        public Position StartPosition { get; set; }
        public Position ExitPosition  { get; set; }

        public MazeGrid(int n)
        {
            Width     = 2 * n + 1;
            Height    = 2 * n + 1;
            _tiles    = new TileType[Width, Height];
            _revealed = new bool[Width, Height];

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    _tiles[x, y] = TileType.Wall;
        }

        public TileType GetTile(int x, int y)     => _tiles[x, y];
        public TileType GetTile(Position p)        => _tiles[p.X, p.Y];

        public void SetTile(int x, int y, TileType type) => _tiles[x, y] = type;
        public void SetTile(Position p,   TileType type) => _tiles[p.X, p.Y] = type;

        public bool InBounds(int x, int y) =>
            x >= 0 && x < Width && y >= 0 && y < Height;

        public bool InBounds(Position p) => InBounds(p.X, p.Y);

        public bool IsWalkable(Position p) =>
            InBounds(p) &&
            _tiles[p.X, p.Y] != TileType.Wall &&
            _tiles[p.X, p.Y] != TileType.LockedExit;

        public bool IsRevealed(int x, int y) => _revealed[x, y];

        public static bool IsInSight(Position center, int x, int y, int radius)
        {
            int dx = x - center.X;
            int dy = y - center.Y;
            return dx * dx + dy * dy <= radius * radius;
        }

        public void Reveal(Position center, int radius)
        {
            int xMin = Math.Max(0, center.X - radius);
            int xMax = Math.Min(Width  - 1, center.X + radius);
            int yMin = Math.Max(0, center.Y - radius);
            int yMax = Math.Min(Height - 1, center.Y + radius);

            for (int x = xMin; x <= xMax; x++)
                for (int y = yMin; y <= yMax; y++)
                    if (IsInSight(center, x, y, radius))
                        _revealed[x, y] = true;
        }

        public List<Position> GetPathTiles()
        {
            var list = new List<Position>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (_tiles[x, y] == TileType.Path)
                        list.Add(new Position(x, y));
            return list;
        }
    }
}
