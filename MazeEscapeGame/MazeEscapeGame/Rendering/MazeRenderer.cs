using System;
using System.Collections.Generic;
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

        private static readonly Color ColourWall       = new Color( 28,  28,  45);
        private static readonly Color ColourPath       = new Color(185, 178, 160);
        private static readonly Color ColourStart      = new Color( 80, 200,  80);
        private static readonly Color ColourExitBase   = new Color(255, 220,  40);
        private static readonly Color ColourExitPulse  = new Color(255, 255, 130);
        private static readonly Color ColourLockedExit = new Color(180,  80,   0);
        private static readonly Color ColourKeyBase    = new Color(  0, 220, 200);
        private static readonly Color ColourKeyShimmer = new Color(180, 255, 255);
        private static readonly Color ColourTrap       = new Color(160,   0, 180);
        private static readonly Color ColourCoin       = new Color(255, 200,   0);
        private static readonly Color ColourFog        = new Color(  5,   5,  10);
        private static readonly Color ColourPlayer     = new Color( 60, 130, 255);
        private static readonly Color ColourEnemy      = new Color(220,  50,  50);

        public MazeRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(MazeGrid grid, Player player, List<Enemy> enemies,
                         double totalSeconds, int camOffsetX, int camOffsetY)
        {
            int ts        = GameSettings.TileSize;
            int fogR      = GameSettings.FogRadius;
            var playerPos = player.Position;

            float exitPulse  = (float)(Math.Sin(totalSeconds * 3.5) * 0.5 + 0.5);
            float keyShimmer = (float)(Math.Sin(totalSeconds * 5.0) * 0.5 + 0.5);
            Color exitColour = Color.Lerp(ColourExitBase,  ColourExitPulse,  exitPulse  * 0.6f);
            Color keyColour  = Color.Lerp(ColourKeyBase,   ColourKeyShimmer, keyShimmer * 0.45f);

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    bool inSight  = MazeGrid.IsInSight(playerPos, x, y, fogR);
                    bool revealed = grid.IsRevealed(x, y);

                    if (!revealed)
                    {
                        _spriteBatch.Draw(_pixel,
                            new Rectangle(x * ts - camOffsetX, y * ts - camOffsetY, ts, ts),
                            ColourFog);
                        continue;
                    }

                    var tileType = grid.GetTile(x, y);
                    Color base_  = TileColour(tileType, exitColour, keyColour);
                    Color drawn  = inSight ? base_ : Dim(base_, 0.35f);

                    bool isWall = tileType == TileType.Wall;
                    int w = isWall ? ts : ts - 1;
                    int h = isWall ? ts : ts - 1;

                    _spriteBatch.Draw(_pixel,
                        new Rectangle(x * ts - camOffsetX, y * ts - camOffsetY, w, h),
                        drawn);
                }
            }

            foreach (var enemy in enemies)
            {
                if (!MazeGrid.IsInSight(playerPos, enemy.Position.X, enemy.Position.Y, fogR))
                    continue;

                const int eInset = 4;
                _spriteBatch.Draw(_pixel,
                    new Rectangle(
                        enemy.Position.X * ts - camOffsetX + eInset,
                        enemy.Position.Y * ts - camOffsetY + eInset,
                        ts - eInset * 2,
                        ts - eInset * 2),
                    ColourEnemy);

                const int dot = 4;
                _spriteBatch.Draw(_pixel,
                    new Rectangle(
                        enemy.Position.X * ts - camOffsetX + ts / 2 - dot / 2,
                        enemy.Position.Y * ts - camOffsetY + ts / 2 - dot / 2,
                        dot, dot),
                    Color.White);
            }

            const int pInset = 5;
            _spriteBatch.Draw(_pixel,
                new Rectangle(
                    playerPos.X * ts - camOffsetX + pInset,
                    playerPos.Y * ts - camOffsetY + pInset,
                    ts - pInset * 2,
                    ts - pInset * 2),
                ColourPlayer);

            const int pdot = 4;
            _spriteBatch.Draw(_pixel,
                new Rectangle(
                    playerPos.X * ts - camOffsetX + ts / 2 - pdot / 2,
                    playerPos.Y * ts - camOffsetY + ts / 2 - pdot / 2,
                    pdot, pdot),
                Color.White);
        }

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

        private static Color TileColour(TileType tile, Color exitColour, Color keyColour) =>
            tile switch
            {
                TileType.Wall       => ColourWall,
                TileType.Path       => ColourPath,
                TileType.Start      => ColourStart,
                TileType.Exit       => exitColour,
                TileType.LockedExit => ColourLockedExit,
                TileType.Key        => keyColour,
                TileType.Trap       => ColourTrap,
                TileType.Coin       => ColourCoin,
                _                   => ColourPath,
            };

        private static Color Dim(Color c, float factor) =>
            new Color((int)(c.R * factor), (int)(c.G * factor), (int)(c.B * factor), c.A);
    }
}
