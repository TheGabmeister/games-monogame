using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong;

public class Game1 : Game
{
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private const int WinScore = 10;

    private const float PaddleWidth = 20f;
    private const float PaddleHeight = 120f;
    private const float PaddleMargin = 48f;
    private const float PaddleSpeed = 520f;

    private const float BallSize = 18f;
    private const float BallSpeed = 460f;
    private const float ServeDelaySeconds = 0.85f;

    private readonly GraphicsDeviceManager _graphics;
    private readonly Random _random = new();

    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private SpriteFont _font;

    private Paddle _leftPaddle;
    private Paddle _rightPaddle;
    private Ball _ball;

    private KeyboardState _currentKeyboard;
    private KeyboardState _previousKeyboard;

    private GameState _gameState = GameState.StartScreen;
    private int _leftScore;
    private int _rightScore;
    private string _winnerText = string.Empty;
    private float _serveTimer;
    private int _nextServeDirection = 1;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = WindowWidth;
        _graphics.PreferredBackBufferHeight = WindowHeight;
        _graphics.SynchronizeWithVerticalRetrace = true;

        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        IsFixedTimeStep = true;

        Window.Title = "Pong";
    }

    protected override void Initialize()
    {
        CreateGameObjects();
        ResetEntities();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("DefaultFont");

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        _currentKeyboard = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            _currentKeyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        bool enterPressed = IsNewKeyPress(Keys.Enter);

        switch (_gameState)
        {
            case GameState.StartScreen:
                if (enterPressed)
                {
                    StartNewMatch();
                }

                break;

            case GameState.Playing:
                UpdatePlaying(gameTime);
                break;

            case GameState.GameOver:
                if (enterPressed)
                {
                    StartNewMatch();
                }

                break;
        }

        _previousKeyboard = _currentKeyboard;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(9, 14, 28));

        _spriteBatch.Begin();

        switch (_gameState)
        {
            case GameState.StartScreen:
                DrawStartScreen(gameTime);
                break;

            case GameState.Playing:
                DrawArena();
                DrawScores();

                if (_serveTimer > 0f)
                {
                    DrawCenteredText("Get Ready", new Vector2(ViewportCenterX(), 110f), Color.WhiteSmoke, 0.95f);
                }

                break;

            case GameState.GameOver:
                DrawArena();
                DrawScores();
                DrawGameOverOverlay(gameTime);
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void UpdatePlaying(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        UpdatePaddles(deltaTime);

        if (_serveTimer > 0f)
        {
            _serveTimer = Math.Max(0f, _serveTimer - deltaTime);

            if (_serveTimer <= 0f)
            {
                LaunchBall();
            }

            return;
        }

        _ball.Update(deltaTime);
        HandleWallCollisions();
        HandlePaddleCollisions();
        HandleScoring();
    }

    private void UpdatePaddles(float deltaTime)
    {
        float leftDirection = 0f;
        float rightDirection = 0f;

        if (_currentKeyboard.IsKeyDown(Keys.W))
        {
            leftDirection -= 1f;
        }

        if (_currentKeyboard.IsKeyDown(Keys.S))
        {
            leftDirection += 1f;
        }

        if (_currentKeyboard.IsKeyDown(Keys.Up))
        {
            rightDirection -= 1f;
        }

        if (_currentKeyboard.IsKeyDown(Keys.Down))
        {
            rightDirection += 1f;
        }

        float screenHeight = GraphicsDevice.Viewport.Height;
        _leftPaddle.Move(leftDirection, deltaTime, screenHeight);
        _rightPaddle.Move(rightDirection, deltaTime, screenHeight);
    }

    private void HandleWallCollisions()
    {
        int screenHeight = GraphicsDevice.Viewport.Height;

        if (_ball.Position.Y <= 0f)
        {
            _ball.Position = new Vector2(_ball.Position.X, 0f);
            _ball.Velocity = new Vector2(_ball.Velocity.X, Math.Abs(_ball.Velocity.Y));
        }
        else if (_ball.Position.Y + _ball.Size >= screenHeight)
        {
            _ball.Position = new Vector2(_ball.Position.X, screenHeight - _ball.Size);
            _ball.Velocity = new Vector2(_ball.Velocity.X, -Math.Abs(_ball.Velocity.Y));
        }
    }

    private void HandlePaddleCollisions()
    {
        Rectangle ballBounds = _ball.Bounds;
        Rectangle leftBounds = _leftPaddle.Bounds;
        Rectangle rightBounds = _rightPaddle.Bounds;

        if (_ball.Velocity.X < 0f && ballBounds.Intersects(leftBounds))
        {
            _ball.Position = new Vector2(leftBounds.Right, _ball.Position.Y);
            BounceBallFromPaddle(_leftPaddle, bounceRight: true);
        }
        else if (_ball.Velocity.X > 0f && ballBounds.Intersects(rightBounds))
        {
            _ball.Position = new Vector2(rightBounds.Left - _ball.Size, _ball.Position.Y);
            BounceBallFromPaddle(_rightPaddle, bounceRight: false);
        }
    }

    private void BounceBallFromPaddle(Paddle paddle, bool bounceRight)
    {
        float paddleCenter = paddle.Position.Y + (paddle.Height * 0.5f);
        float ballCenter = _ball.Position.Y + (_ball.Size * 0.5f);
        float normalizedOffset = MathHelper.Clamp(
            (ballCenter - paddleCenter) / (paddle.Height * 0.5f),
            -1f,
            1f);

        Vector2 direction = new(bounceRight ? 1f : -1f, normalizedOffset * 0.9f);
        direction.Normalize();

        _ball.Velocity = direction * BallSpeed;
    }

    private void HandleScoring()
    {
        int screenWidth = GraphicsDevice.Viewport.Width;

        if (_ball.Position.X + _ball.Size < 0f)
        {
            AwardPoint(leftPlayerScored: false);
        }
        else if (_ball.Position.X > screenWidth)
        {
            AwardPoint(leftPlayerScored: true);
        }
    }

    private void AwardPoint(bool leftPlayerScored)
    {
        if (leftPlayerScored)
        {
            _leftScore++;
        }
        else
        {
            _rightScore++;
        }

        if (_leftScore >= WinScore || _rightScore >= WinScore)
        {
            _winnerText = _leftScore >= WinScore ? "Left Player Wins!" : "Right Player Wins!";
            _gameState = GameState.GameOver;
            _serveTimer = 0f;
            ResetEntities();
            return;
        }

        ResetRound();
    }

    private void StartNewMatch()
    {
        _leftScore = 0;
        _rightScore = 0;
        _winnerText = string.Empty;
        _gameState = GameState.Playing;

        ResetRound();
    }

    private void ResetRound()
    {
        ResetEntities();
        _nextServeDirection = _random.Next(2) == 0 ? -1 : 1;
        _serveTimer = ServeDelaySeconds;
    }

    private void ResetEntities()
    {
        Vector2 leftStart = GetLeftPaddleStart();
        Vector2 rightStart = GetRightPaddleStart();
        Vector2 ballStart = GetBallStart();

        _leftPaddle.Reset(leftStart);
        _rightPaddle.Reset(rightStart);
        _ball.Reset(ballStart);
    }

    private void LaunchBall()
    {
        float verticalDirection = MathHelper.Lerp(-0.75f, 0.75f, (float)_random.NextDouble());
        Vector2 direction = new(_nextServeDirection, verticalDirection);
        direction.Normalize();

        _ball.Velocity = direction * BallSpeed;
    }

    private void CreateGameObjects()
    {
        _leftPaddle = new Paddle(GetLeftPaddleStart(), PaddleWidth, PaddleHeight, PaddleSpeed);
        _rightPaddle = new Paddle(GetRightPaddleStart(), PaddleWidth, PaddleHeight, PaddleSpeed);
        _ball = new Ball(GetBallStart(), BallSize);
    }

    private Vector2 GetLeftPaddleStart()
    {
        float centeredY = (GraphicsDevice.Viewport.Height - PaddleHeight) * 0.5f;
        return new Vector2(PaddleMargin, centeredY);
    }

    private Vector2 GetRightPaddleStart()
    {
        float x = GraphicsDevice.Viewport.Width - PaddleMargin - PaddleWidth;
        float centeredY = (GraphicsDevice.Viewport.Height - PaddleHeight) * 0.5f;
        return new Vector2(x, centeredY);
    }

    private Vector2 GetBallStart()
    {
        float x = (GraphicsDevice.Viewport.Width - BallSize) * 0.5f;
        float y = (GraphicsDevice.Viewport.Height - BallSize) * 0.5f;
        return new Vector2(x, y);
    }

    private bool IsNewKeyPress(Keys key)
    {
        return _currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    }

    private void DrawArena()
    {
        Viewport viewport = GraphicsDevice.Viewport;
        int centerX = viewport.Width / 2;

        for (int y = 24; y < viewport.Height; y += 32)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(centerX - 2, y, 4, 18), new Color(255, 255, 255, 70));
        }

        DrawRectangle(_leftPaddle.Bounds, Color.WhiteSmoke);
        DrawRectangle(_rightPaddle.Bounds, Color.WhiteSmoke);
        DrawRectangle(_ball.Bounds, Color.White);
    }

    private void DrawScores()
    {
        float centerX = ViewportCenterX();
        DrawCenteredText(_leftScore.ToString(), new Vector2(centerX - 90f, 48f), Color.WhiteSmoke, 1.25f);
        DrawCenteredText(_rightScore.ToString(), new Vector2(centerX + 90f, 48f), Color.WhiteSmoke, 1.25f);
    }

    private void DrawStartScreen(GameTime gameTime)
    {
        Viewport viewport = GraphicsDevice.Viewport;

        DrawCenteredText("PONG", new Vector2(ViewportCenterX(), 138f), Color.White, 2.4f);

        Rectangle panel = new(
            (viewport.Width / 2) - 270,
            (viewport.Height / 2) - 120,
            540,
            240);

        DrawPanel(panel, new Color(20, 30, 56, 220), new Color(255, 255, 255, 70));

        DrawCenteredText("First to 10 wins", new Vector2(ViewportCenterX(), panel.Top + 52), Color.WhiteSmoke, 0.95f);
        DrawCenteredText("Left Paddle: W / S", new Vector2(ViewportCenterX(), panel.Top + 102), Color.WhiteSmoke, 0.85f);
        DrawCenteredText("Right Paddle: Up / Down", new Vector2(ViewportCenterX(), panel.Top + 142), Color.WhiteSmoke, 0.85f);

        float pulse = 0.72f + (0.28f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 4f));
        Color promptColor = Color.Lerp(Color.LightSkyBlue, Color.White, pulse);
        DrawCenteredText("Press Enter to Start", new Vector2(ViewportCenterX(), panel.Top + 194), promptColor, 0.92f);
    }

    private void DrawGameOverOverlay(GameTime gameTime)
    {
        Viewport viewport = GraphicsDevice.Viewport;
        Rectangle panel = new(
            (viewport.Width / 2) - 250,
            (viewport.Height / 2) - 110,
            500,
            220);

        DrawRectangle(new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(0, 0, 0, 120));
        DrawPanel(panel, new Color(20, 30, 56, 235), new Color(255, 255, 255, 80));

        DrawCenteredText(_winnerText, new Vector2(ViewportCenterX(), panel.Top + 56), Color.White, 1.15f);
        DrawCenteredText($"Final Score {_leftScore} - {_rightScore}", new Vector2(ViewportCenterX(), panel.Top + 108), Color.WhiteSmoke, 0.82f);

        float pulse = 0.72f + (0.28f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 4f));
        Color promptColor = Color.Lerp(Color.LightSkyBlue, Color.White, pulse);
        DrawCenteredText("Press Enter to Play Again", new Vector2(ViewportCenterX(), panel.Top + 164), promptColor, 0.82f);
    }

    private void DrawPanel(Rectangle bounds, Color fillColor, Color borderColor)
    {
        DrawRectangle(bounds, fillColor);
        DrawRectangle(new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), borderColor);
        DrawRectangle(new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2), borderColor);
        DrawRectangle(new Rectangle(bounds.X, bounds.Y, 2, bounds.Height), borderColor);
        DrawRectangle(new Rectangle(bounds.Right - 2, bounds.Y, 2, bounds.Height), borderColor);
    }

    private void DrawRectangle(Rectangle bounds, Color color)
    {
        _spriteBatch.Draw(_pixel, bounds, color);
    }

    private void DrawCenteredText(string text, Vector2 centerPosition, Color color, float scale)
    {
        Vector2 size = _font.MeasureString(text) * scale;
        Vector2 drawPosition = centerPosition - (size * 0.5f);
        _spriteBatch.DrawString(_font, text, drawPosition, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private float ViewportCenterX()
    {
        return GraphicsDevice.Viewport.Width * 0.5f;
    }
}
