using MazeEscapeGame.Models;

namespace MazeEscapeGame.Core
{
    // The grid uses a (2n+1) x (2n+1) layout where:
    //   - Odd-indexed cells are walkable rooms carved by the generator
    //   - Even-indexed cells are walls unless carved as a passage
    // Cell (cx, cy) in generator space maps to tile (2*cx+1, 2*cy+1).
    public class MazeGrid
    {
        private readonly TileType[,] _tiles;

        public int Width  { get; }
        public int Height { get; }

        public Position StartPosition { get; set; }
        public Position ExitPosition  { get; set; }

        public MazeGrid(int n)
        {
            Width  = 2 * n + 1;
            Height = 2 * n + 1;
            _tiles = new TileType[Width, Height];

            // Everything starts as a wall; the generator carves paths.
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
            InBounds(p) && _tiles[p.X, p.Y] != TileType.Wall;
    }
}
