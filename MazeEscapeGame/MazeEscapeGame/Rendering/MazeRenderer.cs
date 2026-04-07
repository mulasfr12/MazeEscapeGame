using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MazeEscapeGame.Core;
using MazeEscapeGame.Models;

namespace MazeEscapeGame.Rendering
{
    public class MazeRenderer
    {
        private readonly Texture2D   _pixel;
        private readonly SpriteBatch _spriteBatch;

        // Tile colours — dark dungeon palette
        private static readonly Color ColourWall   = new Color( 28,  28,  45);
        private static readonly Color ColourPath   = new Color(185, 178, 160);
        private static readonly Color ColourStart  = new Color( 80, 200,  80);
        private static readonly Color ColourExit   = new Color(255, 220,  40);
        private static readonly Color ColourPlayer = new Color( 60, 130, 255);

        public MazeRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;

            // A single white 1x1 pixel; we tint it to draw any coloured rectangle.
            // This is the standard MonoGame approach — there is no built-in shape API.
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        // Draws the full maze then the player on top.
        // cameraOffset is calculated by GetCameraOffset and is already clamped.
        public void Draw(MazeGrid grid, Player player, int camOffsetX, int camOffsetY)
        {
            int ts = GameSettings.TileSize;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var colour = grid.GetTile(x, y) switch
                    {
                        TileType.Wall  => ColourWall,
                        TileType.Path  => ColourPath,
                        TileType.Start => ColourStart,
                        TileType.Exit  => ColourExit,
                        _              => ColourPath,
                    };

                    _spriteBatch.Draw(
                        _pixel,
                        new Rectangle(x * ts - camOffsetX, y * ts - camOffsetY, ts, ts),
                        colour);
                }
            }

            // Player is drawn inset by 4px on each side so it sits visually inside the tile.
            int inset = 4;
            _spriteBatch.Draw(
                _pixel,
                new Rectangle(
                    player.Position.X * ts - camOffsetX + inset,
                    player.Position.Y * ts - camOffsetY + inset,
                    ts - inset * 2,
                    ts - inset * 2),
                ColourPlayer);
        }

        // Returns a camera offset that centres on the player, clamped so the
        // viewport never shows space outside the maze boundaries.
        public (int x, int y) GetCameraOffset(Position playerPos, MazeGrid grid,
                                               int screenWidth, int screenHeight)
        {
            int ts = GameSettings.TileSize;

            int camX = playerPos.X * ts - screenWidth  / 2 + ts / 2;
            int camY = playerPos.Y * ts - screenHeight / 2 + ts / 2;

            int maxCamX = grid.Width  * ts - screenWidth;
            int maxCamY = grid.Height * ts - screenHeight;

            camX = Math.Clamp(camX, 0, Math.Max(0, maxCamX));
            camY = Math.Clamp(camY, 0, Math.Max(0, maxCamY));

            return (camX, camY);
        }
    }
}
