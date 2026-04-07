using Microsoft.Xna.Framework.Input;
using MazeEscapeGame.Enums;

namespace MazeEscapeGame.Core
{
    // Tracks current and previous keyboard state so callers can distinguish
    // "just pressed" from "held down" without duplicating that logic elsewhere.
    public class InputHandler
    {
        private KeyboardState _current;
        private KeyboardState _previous;

        public void Update()
        {
            _previous = _current;
            _current  = Keyboard.GetState();
        }

        // True only on the frame the key transitions from up → down.
        public bool WasPressed(Keys key) =>
            _current.IsKeyDown(key) && _previous.IsKeyUp(key);

        public bool IsHeld(Keys key) => _current.IsKeyDown(key);

        // Returns the first movement direction pressed this frame, or null.
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
