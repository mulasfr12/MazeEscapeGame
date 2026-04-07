using MazeEscapeGame.Enums;
using MazeEscapeGame.Models;

namespace MazeEscapeGame.Core
{
    public class Player
    {
        public Position Position { get; private set; }

        public Player(Position startPosition)
        {
            Position = startPosition;
        }

        // Returns true if the move succeeded (target tile is walkable).
        public bool TryMove(Direction direction, MazeGrid grid)
        {
            var newPos = Position + DirectionToDelta(direction);
            if (!grid.IsWalkable(newPos))
                return false;

            Position = newPos;
            return true;
        }

        private static Position DirectionToDelta(Direction direction) => direction switch
        {
            Direction.Up    => new Position( 0, -1),
            Direction.Down  => new Position( 0,  1),
            Direction.Left  => new Position(-1,  0),
            Direction.Right => new Position( 1,  0),
            _               => new Position( 0,  0),
        };
    }
}
