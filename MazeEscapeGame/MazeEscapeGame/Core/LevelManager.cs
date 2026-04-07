using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MazeEscapeGame.Enums;
using MazeEscapeGame.Models;

namespace MazeEscapeGame.Core
{
    public class LevelManager
    {
        private readonly Random        _random;
        private readonly MazeGenerator _generator;

        public int         CurrentLevel  { get; private set; }
        public MazeGrid    CurrentMaze   { get; private set; }
        public Player      Player        { get; private set; }
        public List<Enemy> Enemies       { get; private set; }

        // Timer
        public double TimeRemaining { get; private set; }
        public bool   IsTimeUp      => TimeRemaining <= 0;

        // Score (cumulative across levels in the current run)
        public int TotalScore { get; private set; }

        // Phase 3 state
        public bool HasKey          { get; private set; }
        public bool IsPlayerCaught  { get; private set; }

        private static double TimeLimitForLevel(int level) => 45.0 + level * 15.0;

        public LevelManager()
        {
            _random    = new Random();
            _generator = new MazeGenerator(_random);
        }

        // -------------------------------------------------------------------------

        public void LoadLevel(int level)
        {
            CurrentLevel   = level;
            TimeRemaining  = TimeLimitForLevel(level);
            HasKey         = false;
            IsPlayerCaught = false;
            Enemies        = new List<Enemy>();

            int n       = GameSettings.MazeCellsForLevel(level);
            CurrentMaze = new MazeGrid(n);
            _generator.Generate(CurrentMaze);

            PlaceItems();

            Player = new Player(CurrentMaze.StartPosition);

            // Reveal the starting area immediately
            CurrentMaze.Reveal(Player.Position, GameSettings.FogRadius);
        }

        public void ResetRun()
        {
            TotalScore = 0;
            LoadLevel(1);
        }

        public void NextLevel() => LoadLevel(CurrentLevel + 1);

        // -------------------------------------------------------------------------

        // Called every frame while Playing.
        public void Update(GameTime gameTime)
        {
            if (TimeRemaining > 0)
                TimeRemaining -= gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var enemy in Enemies)
            {
                enemy.Update(gameTime, CurrentMaze);
                if (enemy.Position == Player.Position)
                    IsPlayerCaught = true;
            }
        }

        // Handles movement plus all tile-interaction logic.
        // Game1 calls this instead of Player.TryMove directly.
        public void MovePlayer(Direction direction)
        {
            var delta     = DirectionToDelta(direction);
            var targetPos = Player.Position + delta;

            if (!CurrentMaze.InBounds(targetPos)) return;

            // LockedExit: only open it if the player has collected the key
            if (CurrentMaze.GetTile(targetPos) == TileType.LockedExit)
            {
                if (!HasKey) return;
                CurrentMaze.SetTile(targetPos, TileType.Exit); // unlock
            }

            if (!Player.TryMove(direction, CurrentMaze)) return;

            // Reveal new area
            CurrentMaze.Reveal(Player.Position, GameSettings.FogRadius);

            // Tile effects on landing
            switch (CurrentMaze.GetTile(Player.Position))
            {
                case TileType.Key:
                    HasKey = true;
                    CurrentMaze.SetTile(Player.Position, TileType.Path);
                    break;

                case TileType.Coin:
                    TotalScore += 50;
                    CurrentMaze.SetTile(Player.Position, TileType.Path);
                    break;

                case TileType.Trap:
                    TimeRemaining = Math.Max(0, TimeRemaining - 10.0);
                    break;
            }

            // Enemy collision after player moves into their tile
            foreach (var enemy in Enemies)
                if (enemy.Position == Player.Position)
                    IsPlayerCaught = true;
        }

        // -------------------------------------------------------------------------

        public void BankLevelScore()
        {
            int earned = (int)(TimeRemaining * 10) + CurrentLevel * 100;
            TotalScore += Math.Max(0, earned);
        }

        public bool PlayerReachedExit() =>
            Player.Position == CurrentMaze.ExitPosition;

        // -------------------------------------------------------------------------
        // Item and enemy placement

        private void PlaceItems()
        {
            var pathTiles = CurrentMaze.GetPathTiles();
            Shuffle(pathTiles);

            // Start and exit positions are non-Path tiles so they won't appear in
            // pathTiles, but we still track them as reserved to skip nearby slots
            // if needed in future. Currently used as an explicit safety guard.
            var reserved = new HashSet<Position>
            {
                CurrentMaze.StartPosition,
                CurrentMaze.ExitPosition,
            };

            // Key — always one per level
            Place(TileType.Key, 1, pathTiles, reserved);

            // Traps — scale with level
            Place(TileType.Trap, CurrentLevel + 1, pathTiles, reserved);

            // Coins — fixed count
            Place(TileType.Coin, 3, pathTiles, reserved);

            // Enemies — level 1 has none, caps at 3
            int enemyCount = Math.Min(CurrentLevel - 1, 3);
            for (int i = 0; i < enemyCount; i++)
            {
                var pos = PickNext(pathTiles, reserved);
                if (pos is null) break;
                Enemies.Add(new Enemy(pos.Value, _random));
                reserved.Add(pos.Value);
            }
        }

        private void Place(TileType type, int count, List<Position> tiles, HashSet<Position> reserved)
        {
            for (int i = 0; i < count; i++)
            {
                var pos = PickNext(tiles, reserved);
                if (pos is null) break;
                CurrentMaze.SetTile(pos.Value, type);
                reserved.Add(pos.Value);
            }
        }

        // Returns the next position in the (already shuffled) list that isn't reserved.
        private static Position? PickNext(List<Position> tiles, HashSet<Position> reserved)
        {
            foreach (var p in tiles)
                if (!reserved.Contains(p) && /* still Path */ true)
                    return p;
            return null;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static Position DirectionToDelta(Direction d) => d switch
        {
            Direction.Up    => new Position( 0, -1),
            Direction.Down  => new Position( 0,  1),
            Direction.Left  => new Position(-1,  0),
            Direction.Right => new Position( 1,  0),
            _               => new Position( 0,  0),
        };
    }
}
