namespace MazeEscapeGame.Models
{
    public static class GameSettings
    {
        public const int TileSize     = 32;
        public const int WindowWidth  = 800;
        public const int WindowHeight = 600;

        // Returns n where the maze grid is (2n+1) x (2n+1).
        // Level 1 → n=8 → 17x17 tiles (544px, fits without camera)
        // Each level adds 2, so level 5 → n=16 → 33x33 (needs camera)
        public static int MazeCellsForLevel(int level) => 6 + level * 2;

        // Fog-of-war: tile radius the player can see around themselves (circular)
        public const int FogRadius = 5;
    }
}
