using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MazeEscapeGame.Core;
using MazeEscapeGame.Models;
using MazeEscapeGame.Rendering;

namespace MazeEscapeGame
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch  _spriteBatch;
        private SpriteFont   _font;

        private LevelManager _levelManager;
        private InputHandler _inputHandler;
        private MazeRenderer _mazeRenderer;
        private Texture2D    _overlayPixel;

        private GameState _state;
        private string    _gameOverReason = "";

        // -------------------------------------------------------------------------

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth  = GameSettings.WindowWidth;
            _graphics.PreferredBackBufferHeight = GameSettings.WindowHeight;
        }

        protected override void Initialize()
        {
            _inputHandler = new InputHandler();
            _levelManager = new LevelManager();
            _state        = GameState.Start;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch  = new SpriteBatch(GraphicsDevice);
            _font         = Content.Load<SpriteFont>("DefaultFont");
            _mazeRenderer = new MazeRenderer(GraphicsDevice, _spriteBatch);

            _overlayPixel = new Texture2D(GraphicsDevice, 1, 1);
            _overlayPixel.SetData(new[] { Color.White });
        }

        // -------------------------------------------------------------------------

        protected override void Update(GameTime gameTime)
        {
            _inputHandler.Update();

            if (_inputHandler.WasPressed(Keys.Escape))
                Exit();

            switch (_state)
            {
                case GameState.Start:
                    if (_inputHandler.WasPressed(Keys.Enter))
                    {
                        _levelManager.ResetRun();
                        _state = GameState.Playing;
                    }
                    break;

                case GameState.Playing:
                    _levelManager.Update(gameTime);

                    if (CheckGameOver()) break;

                    var dir = _inputHandler.GetMoveDirection();
                    if (dir.HasValue)
                    {
                        _levelManager.MovePlayer(dir.Value);

                        if (CheckGameOver()) break;

                        if (_levelManager.PlayerReachedExit())
                        {
                            _levelManager.BankLevelScore();
                            _state = GameState.LevelComplete;
                        }
                    }
                    break;

                case GameState.LevelComplete:
                    if (_inputHandler.WasPressed(Keys.Enter))
                    {
                        _levelManager.NextLevel();
                        _state = GameState.Playing;
                    }
                    break;

                case GameState.GameOver:
                    if (_inputHandler.WasPressed(Keys.Enter))
                    {
                        _levelManager.ResetRun();
                        _state = GameState.Playing;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        // Returns true and transitions to GameOver if a lose condition is met.
        private bool CheckGameOver()
        {
            if (_levelManager.IsPlayerCaught || _levelManager.IsTimeUp)
            {
                _gameOverReason = _levelManager.IsPlayerCaught
                    ? "Caught by an enemy!"
                    : "Time's up!";
                _state = GameState.GameOver;
                return true;
            }
            return false;
        }

        // -------------------------------------------------------------------------

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(15, 15, 25));

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            switch (_state)
            {
                case GameState.Start:
                    DrawStartScreen();
                    break;

                case GameState.Playing:
                    DrawGame();
                    break;

                case GameState.LevelComplete:
                    DrawGame();
                    DrawLevelCompleteOverlay();
                    break;

                case GameState.GameOver:
                    DrawGameOverScreen();
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        // -------------------------------------------------------------------------

        private void DrawGame()
        {
            var (camX, camY) = _mazeRenderer.GetCameraOffset(
                _levelManager.Player.Position,
                _levelManager.CurrentMaze,
                GameSettings.WindowWidth,
                GameSettings.WindowHeight);

            _mazeRenderer.Draw(
                _levelManager.CurrentMaze,
                _levelManager.Player,
                _levelManager.Enemies,
                camX, camY);

            DrawHud();
        }

        private void DrawHud()
        {
            // Level — top left
            _spriteBatch.DrawString(_font,
                $"Level: {_levelManager.CurrentLevel}",
                new Vector2(10, 10), Color.White);

            // Key indicator — below level
            string keyText   = _levelManager.HasKey ? "KEY: FOUND" : "KEY: NEEDED";
            Color  keyColour = _levelManager.HasKey ? ColourCyan : new Color(200, 140, 40);
            _spriteBatch.DrawString(_font, keyText, new Vector2(10, 30), keyColour);

            // Timer — top centre, turns red under 10 s
            int secs    = Math.Max(0, (int)Math.Ceiling(_levelManager.TimeRemaining));
            string timer = $"{secs / 60}:{secs % 60:D2}";
            Color timerColour = secs <= 10 ? ColourRed : Color.White;
            var timerSize = _font.MeasureString(timer);
            _spriteBatch.DrawString(_font, timer,
                new Vector2((GameSettings.WindowWidth - timerSize.X) / 2f, 10),
                timerColour);

            // Score — top right
            _spriteBatch.DrawString(_font,
                $"Score: {_levelManager.TotalScore}",
                new Vector2(GameSettings.WindowWidth - 120, 10), Color.White);
        }

        private void DrawStartScreen()
        {
            DrawCentred("MAZE ESCAPE",            GameSettings.WindowHeight / 2 - 85, ColourYellow);
            DrawCentred("Press ENTER to begin",   GameSettings.WindowHeight / 2 - 30, Color.White);
            DrawCentred("WASD / Arrows  -  move", GameSettings.WindowHeight / 2 + 25, ColourGray);
            DrawCentred("Find the KEY, reach the EXIT, survive!", GameSettings.WindowHeight / 2 + 52, ColourGray);
            DrawCentred("ESC  -  quit",            GameSettings.WindowHeight / 2 + 78, ColourGray);
        }

        private void DrawLevelCompleteOverlay()
        {
            DrawRect(0, GameSettings.WindowHeight / 2 - 55,
                     GameSettings.WindowWidth, 110, new Color(0, 0, 0, 190));

            DrawCentred($"LEVEL {_levelManager.CurrentLevel} COMPLETE!",
                GameSettings.WindowHeight / 2 - 35, ColourYellow);
            DrawCentred($"Score: {_levelManager.TotalScore}",
                GameSettings.WindowHeight / 2 - 8, Color.White);
            DrawCentred("Press ENTER for next level",
                GameSettings.WindowHeight / 2 + 20, ColourGray);
        }

        private void DrawGameOverScreen()
        {
            DrawCentred("GAME OVER",              GameSettings.WindowHeight / 2 - 55, ColourRed);
            DrawCentred(_gameOverReason,           GameSettings.WindowHeight / 2 - 25, new Color(220, 180, 100));
            DrawCentred($"Final score: {_levelManager.TotalScore}",
                GameSettings.WindowHeight / 2 + 5, Color.White);
            DrawCentred($"Reached level {_levelManager.CurrentLevel}",
                GameSettings.WindowHeight / 2 + 28, ColourGray);
            DrawCentred("Press ENTER to restart",
                GameSettings.WindowHeight / 2 + 58, ColourGray);
        }

        // -------------------------------------------------------------------------

        private void DrawCentred(string text, int y, Color colour)
        {
            var size = _font.MeasureString(text);
            _spriteBatch.DrawString(_font, text,
                new Vector2((GameSettings.WindowWidth - size.X) / 2f, y),
                colour);
        }

        private void DrawRect(int x, int y, int w, int h, Color colour)
        {
            _spriteBatch.Draw(_overlayPixel, new Rectangle(x, y, w, h), colour);
        }

        private static readonly Color ColourYellow = new Color(255, 220,  40);
        private static readonly Color ColourRed    = new Color(220,  60,  60);
        private static readonly Color ColourGray   = new Color(160, 160, 160);
        private static readonly Color ColourCyan   = new Color(  0, 220, 200);
    }
}
