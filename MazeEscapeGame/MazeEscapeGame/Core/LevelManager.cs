using MazeEscapeGame.Models;
using Microsoft.Xna.Framework;
using System;

namespace MazeEscapeGame.Core
{
    public class LevelManager
    {
        private readonly Random        _random;
        private readonly MazeGenerator _generator;

        public int      CurrentLevel  { get; private set; }
        public MazeGrid CurrentMaze   { get; private set; }
        public Player   Player        { get; private set; }

        // Timer
        public double TimeRemaining   { get; private set; }
        public bool   IsTimeUp        => TimeRemaining <= 0;

        // Score accumulated across all levels in the current run
        public int    TotalScore      { get; private set; }

        // Seconds allowed per level — larger mazes get more time
        private static double TimeLimitForLevel(int level) => 45.0 + level * 15.0;

        public LevelManager()
        {
            _random    = new Random();
            _generator = new MazeGenerator(_random);
        }

        public void LoadLevel(int level)
        {
            CurrentLevel  = level;
            TimeRemaining = TimeLimitForLevel(level);

            int n        = GameSettings.MazeCellsForLevel(level);
            CurrentMaze  = new MazeGrid(n);
            _generator.Generate(CurrentMaze);

            Player = new Player(CurrentMaze.StartPosition);
        }

        public void ResetRun()
        {
            TotalScore = 0;
            LoadLevel(1);
        }

        // Call each frame while the player is actively playing.
        public void Update(GameTime gameTime)
        {
            if (TimeRemaining > 0)
                TimeRemaining -= gameTime.ElapsedGameTime.TotalSeconds;
        }

        // Call when the player reaches the exit to bank the level score.
        public void BankLevelScore()
        {
            // Time bonus + flat level reward
            int earned = (int)(TimeRemaining * 10) + CurrentLevel * 100;
            TotalScore += Math.Max(0, earned);
        }

        public void NextLevel() => LoadLevel(CurrentLevel + 1);

        public bool PlayerReachedExit() =>
            Player.Position == CurrentMaze.ExitPosition;
    }
}
