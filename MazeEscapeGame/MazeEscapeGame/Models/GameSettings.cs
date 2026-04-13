namespace MazeEscapeGame.Models
{
    public static class GameSettings
    {
        public const int TileSize     = 32;
        public const int WindowWidth  = 800;
        public const int WindowHeight = 600;

        public static int MazeCellsForLevel(int level) => 6 + level * 2;

        public const int FogRadius = 5;
    }
}
