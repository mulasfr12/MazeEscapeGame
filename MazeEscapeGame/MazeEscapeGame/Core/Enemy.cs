using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MazeEscapeGame.Models;

namespace MazeEscapeGame.Core
{
    // Roaming enemy: moves to a random walkable neighbour every MoveInterval seconds.
    public class Enemy
    {
        private static readonly (int dx, int dy)[] Directions =
            { (0, -1), (0, 1), (-1, 0), (1, 0) };

        private const double MoveInterval = 0.6; // seconds between steps

        public Position Position { get; private set; }

        private readonly Random _random;
        private double _moveTimer;

        public Enemy(Position startPosition, Random random)
        {
            Position = startPosition;
            _random  = random;
        }

        public void Update(GameTime gameTime, MazeGrid grid)
        {
            _moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_moveTimer < MoveInterval) return;

            _moveTimer -= MoveInterval;
            MoveRandom(grid);
        }

        private void MoveRandom(MazeGrid grid)
        {
            var options = new List<Position>(4);
            foreach (var (dx, dy) in Directions)
            {
                var next = new Position(Position.X + dx, Position.Y + dy);
                if (grid.IsWalkable(next))
                    options.Add(next);
            }

            if (options.Count > 0)
                Position = options[_random.Next(options.Count)];
        }
    }
}
