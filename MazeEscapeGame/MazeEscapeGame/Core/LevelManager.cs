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

        public double TimeRemaining { get; private set; }
        public bool   IsTimeUp      => TimeRemaining <= 0;

        public int  TotalScore     { get; private set; }
        public bool HasKey         { get; private set; }
        public bool IsPlayerCaught { get; private set; }
        public bool WasTrapped     { get; private set; }

        private static double TimeLimitForLevel(int level) => 45.0 + level * 15.0;

        public LevelManager()
        {
            _random    = new Random();
            _generator = new MazeGenerator(_random);
        }

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
            CurrentMaze.Reveal(Player.Position, GameSettings.FogRadius);
        }

        public void ResetRun()
        {
            TotalScore = 0;
            LoadLevel(1);
        }

        public void NextLevel() => LoadLevel(CurrentLevel + 1);

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

        public void MovePlayer(Direction direction)
        {
            WasTrapped = false;
            var delta     = DirectionToDelta(direction);
            var targetPos = Player.Position + delta;

            if (!CurrentMaze.InBounds(targetPos)) return;

            if (CurrentMaze.GetTile(targetPos) == TileType.LockedExit)
            {
                if (!HasKey) return;
                CurrentMaze.SetTile(targetPos, TileType.Exit);
            }

            if (!Player.TryMove(direction, CurrentMaze)) return;

            CurrentMaze.Reveal(Player.Position, GameSettings.FogRadius);

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
                    WasTrapped    = true;
                    TimeRemaining = Math.Max(0, TimeRemaining - 10.0);
                    break;
            }

            foreach (var enemy in Enemies)
                if (enemy.Position == Player.Position)
                    IsPlayerCaught = true;
        }

        public void BankLevelScore()
        {
            int earned = (int)(TimeRemaining * 10) + CurrentLevel * 100;
            TotalScore += Math.Max(0, earned);
        }

        public bool PlayerReachedExit() =>
            Player.Position == CurrentMaze.ExitPosition;

        private void PlaceItems()
        {
            var pathTiles = CurrentMaze.GetPathTiles();
            Shuffle(pathTiles);

            var reserved = new HashSet<Position>
            {
                CurrentMaze.StartPosition,
                CurrentMaze.ExitPosition,
            };

            Place(TileType.Key,  1,                pathTiles, reserved);
            Place(TileType.Trap, CurrentLevel + 1, pathTiles, reserved);
            Place(TileType.Coin, 3,                pathTiles, reserved);

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

        private static Position? PickNext(List<Position> tiles, HashSet<Position> reserved)
        {
            foreach (var p in tiles)
                if (!reserved.Contains(p))
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
