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
        private bool      _newHighScore   = false;
        private bool      _showLegend     = false;

        private float _shakeTimer = 0f;
        private const float ShakeDuration  = 0.35f;
        private const int   ShakeAmplitude = 5;

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

            HighScoreManager.Load();

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

        protected override void Update(GameTime gameTime)
        {
            _inputHandler.Update();

            if (_shakeTimer > 0)
                _shakeTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                    if (_inputHandler.WasPressed(Keys.H))
                        _showLegend = !_showLegend;

                    _levelManager.Update(gameTime);

                    if (CheckGameOver()) break;

                    var dir = _inputHandler.GetMoveDirection();
                    if (dir.HasValue)
                    {
                        _levelManager.MovePlayer(dir.Value);

                        if (_levelManager.WasTrapped)
                            StartShake();

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
                        _newHighScore = false;
                        _state        = GameState.Playing;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        private bool CheckGameOver()
        {
            if (!_levelManager.IsPlayerCaught && !_levelManager.IsTimeUp) return false;

            _gameOverReason = _levelManager.IsPlayerCaught
                ? "Caught by an enemy!"
                : "Time's up!";

            _newHighScore = HighScoreManager.TrySave(_levelManager.TotalScore);
            _state        = GameState.GameOver;
            return true;
        }

        private void StartShake() => _shakeTimer = ShakeDuration;

        private (int dx, int dy) ShakeOffset()
        {
            if (_shakeTimer <= 0) return (0, 0);
            double elapsed   = ShakeDuration - _shakeTimer;
            float  intensity = (_shakeTimer / ShakeDuration) * ShakeAmplitude;
            return (
                (int)(Math.Sin(elapsed * 55) * intensity),
                (int)(Math.Cos(elapsed * 47) * intensity)
            );
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(15, 15, 25));

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            switch (_state)
            {
                case GameState.Start:
                    DrawStartScreen(gameTime);
                    break;

                case GameState.Playing:
                    DrawGame(gameTime);
                    if (_showLegend) DrawLegend();
                    break;

                case GameState.LevelComplete:
                    DrawGame(gameTime);
                    DrawLevelCompleteOverlay();
                    break;

                case GameState.GameOver:
                    DrawGameOverScreen();
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGame(GameTime gameTime)
        {
            var (camX, camY) = _mazeRenderer.GetCameraOffset(
                _levelManager.Player.Position,
                _levelManager.CurrentMaze,
                GameSettings.WindowWidth,
                GameSettings.WindowHeight);

            var (shakeX, shakeY) = ShakeOffset();

            _mazeRenderer.Draw(
                _levelManager.CurrentMaze,
                _levelManager.Player,
                _levelManager.Enemies,
                gameTime.TotalGameTime.TotalSeconds,
                camX + shakeX,
                camY + shakeY);

            DrawHud(gameTime);
        }

        private void DrawHud(GameTime gameTime)
        {
            _spriteBatch.DrawString(_font,
                $"Level: {_levelManager.CurrentLevel}",
                new Vector2(10, 10), Color.White);

            string keyText   = _levelManager.HasKey ? "KEY: FOUND" : "KEY: NEEDED";
            Color  keyColour = _levelManager.HasKey ? ColourCyan : new Color(200, 140, 40);
            _spriteBatch.DrawString(_font, keyText, new Vector2(10, 30), keyColour);

            int secs = Math.Max(0, (int)Math.Ceiling(_levelManager.TimeRemaining));
            string timer = $"{secs / 60}:{secs % 60:D2}";
            Color timerColour = secs <= 10
                ? PulseColour(ColourRed, Color.White, gameTime.TotalGameTime.TotalSeconds, 4.0)
                : Color.White;
            var timerSize = _font.MeasureString(timer);
            _spriteBatch.DrawString(_font, timer,
                new Vector2((GameSettings.WindowWidth - timerSize.X) / 2f, 10),
                timerColour);

            _spriteBatch.DrawString(_font,
                $"Score: {_levelManager.TotalScore}",
                new Vector2(GameSettings.WindowWidth - 120, 10), Color.White);

            _spriteBatch.DrawString(_font,
                $"Best:  {HighScoreManager.HighScore}",
                new Vector2(GameSettings.WindowWidth - 120, 30), ColourGray);

            _spriteBatch.DrawString(_font, "H - legend",
                new Vector2(10, GameSettings.WindowHeight - 22), ColourGray);
        }

        private static readonly (Color colour, string label)[] LegendEntries =
        {
            (new Color( 60, 130, 255), "You (player)"),
            (new Color( 80, 200,  80), "Start tile"),
            (new Color(255, 220,  40), "Exit  (unlocked)"),
            (new Color(180,  80,   0), "Exit  (locked - need key)"),
            (new Color(  0, 220, 200), "Key   (unlocks exit)"),
            (new Color(160,   0, 180), "Trap  (-10 seconds)"),
            (new Color(255, 200,   0), "Coin  (+50 score)"),
            (new Color(220,  50,  50), "Enemy (avoid!)"),
        };

        private void DrawLegend()
        {
            const int swatchSize  = 14;
            const int lineHeight  = 22;
            const int padX        = 10;
            const int padY        = 8;
            const int panelX      = 10;
            const int titleHeight = 22;

            int panelW = 235;
            int panelH = padY * 2 + titleHeight + LegendEntries.Length * lineHeight;
            int panelY = GameSettings.WindowHeight - panelH - 28;

            DrawRect(panelX, panelY, panelW, panelH, new Color(0, 0, 0, 210));

            _spriteBatch.DrawString(_font, "LEGEND  (H to hide)",
                new Vector2(panelX + padX, panelY + padY), ColourGray);

            int rowY = panelY + padY + titleHeight;
            foreach (var (colour, label) in LegendEntries)
            {
                DrawRect(panelX + padX, rowY + (lineHeight - swatchSize) / 2,
                         swatchSize, swatchSize, colour);

                _spriteBatch.DrawString(_font, label,
                    new Vector2(panelX + padX + swatchSize + 6, rowY), Color.White);

                rowY += lineHeight;
            }
        }

        private void DrawStartScreen(GameTime gameTime)
        {
            Color titleColour = PulseColour(ColourYellow, new Color(255, 160, 0),
                                            gameTime.TotalGameTime.TotalSeconds, 1.8);

            DrawCentred("MAZE ESCAPE",                                GameSettings.WindowHeight / 2 - 100, titleColour);
            DrawCentred("Press ENTER to begin",                       GameSettings.WindowHeight / 2 -  40, Color.White);
            DrawCentred("WASD / Arrows  -  move",                     GameSettings.WindowHeight / 2 +  20, ColourGray);
            DrawCentred("Find the KEY (cyan)  then reach the EXIT",   GameSettings.WindowHeight / 2 +  45, ColourGray);
            DrawCentred("Avoid traps (purple) and enemies (red)",     GameSettings.WindowHeight / 2 +  68, ColourGray);
            DrawCentred("ESC  -  quit",                               GameSettings.WindowHeight / 2 +  95, ColourGray);

            if (HighScoreManager.HighScore > 0)
                DrawCentred($"Best score: {HighScoreManager.HighScore}",
                    GameSettings.WindowHeight / 2 + 125, ColourYellow);
        }

        private void DrawLevelCompleteOverlay()
        {
            DrawRect(0, GameSettings.WindowHeight / 2 - 55,
                     GameSettings.WindowWidth, 110, new Color(0, 0, 0, 190));

            DrawCentred($"LEVEL {_levelManager.CurrentLevel} COMPLETE!",
                GameSettings.WindowHeight / 2 - 35, ColourYellow);
            DrawCentred($"Score: {_levelManager.TotalScore}",
                GameSettings.WindowHeight / 2 -  8, Color.White);
            DrawCentred("Press ENTER for next level",
                GameSettings.WindowHeight / 2 + 20, ColourGray);
        }

        private void DrawGameOverScreen()
        {
            DrawCentred("GAME OVER",          GameSettings.WindowHeight / 2 - 70, ColourRed);
            DrawCentred(_gameOverReason,       GameSettings.WindowHeight / 2 - 38, new Color(220, 180, 100));
            DrawCentred($"Final score: {_levelManager.TotalScore}",
                GameSettings.WindowHeight / 2 -  8, Color.White);
            DrawCentred($"Reached level {_levelManager.CurrentLevel}",
                GameSettings.WindowHeight / 2 + 18, ColourGray);

            if (_newHighScore)
                DrawCentred("NEW HIGH SCORE!",
                    GameSettings.WindowHeight / 2 + 45, ColourYellow);

            DrawCentred("Press ENTER to restart",
                GameSettings.WindowHeight / 2 + (_newHighScore ? 72 : 48), ColourGray);
        }

        private static Color PulseColour(Color a, Color b, double totalSeconds, double freq)
        {
            float t = (float)(Math.Sin(totalSeconds * freq * Math.PI) * 0.5 + 0.5);
            return Color.Lerp(a, b, t);
        }

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
