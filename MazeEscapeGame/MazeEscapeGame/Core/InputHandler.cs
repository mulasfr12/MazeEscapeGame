using Microsoft.Xna.Framework.Input;
using MazeEscapeGame.Enums;

namespace MazeEscapeGame.Core
{
    public class InputHandler
    {
        private KeyboardState _current;
        private KeyboardState _previous;

        public void Update()
        {
            _previous = _current;
            _current  = Keyboard.GetState();
        }

        public bool WasPressed(Keys key) =>
            _current.IsKeyDown(key) && _previous.IsKeyUp(key);

        public bool IsHeld(Keys key) => _current.IsKeyDown(key);

        public Direction? GetMoveDirection()
        {
            if (WasPressed(Keys.Up)    || WasPressed(Keys.W)) return Direction.Up;
            if (WasPressed(Keys.Down)  || WasPressed(Keys.S)) return Direction.Down;
            if (WasPressed(Keys.Left)  || WasPressed(Keys.A)) return Direction.Left;
            if (WasPressed(Keys.Right) || WasPressed(Keys.D)) return Direction.Right;
            return null;
        }
    }
}
