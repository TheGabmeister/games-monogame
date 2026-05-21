using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly InputState _input = new();
    private readonly List<Bullet> _bullets = new();
    private readonly List<Asteroid> _asteroids = new();

    private SpriteBatch _spriteBatch = null!;
    private RenderTarget2D _virtualTarget = null!;
    private Rectangle _displayRectangle;

    private SpriteFont _uiFont = null!;
    private Texture2D _shipTexture = null!;
    private Texture2D _bulletTexture = null!;
    private Texture2D _thrusterTexture = null!;
    private AsteroidSpawner _asteroidSpawner = null!;
    private PlayerShip _player = null!;

    private SoundEffect _shootSound = null!;
    private SoundEffect _asteroidHitSound = null!;
    private SoundEffect _shipExplosionSound = null!;
    private SoundEffect _waveStartSound = null!;

    private GameState _state = GameState.MainMenu;
    private int _score;
    private int _lives;
    private int _wave;
    private float _deathTimer;
    private float _waveTimer;
    private bool _waitingForWave;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = GameConstants.VirtualWidth,
            PreferredBackBufferHeight = GameConstants.VirtualHeight,
            SynchronizeWithVerticalRetrace = true
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (_, _) => RecalculateDisplayRectangle();
    }

    protected override void Initialize()
    {
        base.Initialize();
        RecalculateDisplayRectangle();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _virtualTarget = new RenderTarget2D(GraphicsDevice, GameConstants.VirtualWidth, GameConstants.VirtualHeight);

        _uiFont = Content.Load<SpriteFont>("fonts/ui");
        _shipTexture = Content.Load<Texture2D>("sprites/player/ship");
        _bulletTexture = Content.Load<Texture2D>("sprites/projectiles/bullet");
        _thrusterTexture = Content.Load<Texture2D>("sprites/effects/thruster_particle");

        Texture2D[] largeAsteroids =
        [
            Content.Load<Texture2D>("sprites/asteroids/large_01"),
                Content.Load<Texture2D>("sprites/asteroids/large_02")
        ];

        Texture2D[] mediumAsteroids =
        [
            Content.Load<Texture2D>("sprites/asteroids/medium_01"),
                Content.Load<Texture2D>("sprites/asteroids/medium_02")
        ];

        Texture2D[] smallAsteroids =
        [
            Content.Load<Texture2D>("sprites/asteroids/small_01"),
                Content.Load<Texture2D>("sprites/asteroids/small_02")
        ];

        _asteroidSpawner = new AsteroidSpawner(largeAsteroids, mediumAsteroids, smallAsteroids);
        _player = new PlayerShip(_shipTexture);

        _shootSound = Content.Load<SoundEffect>("audio/sfx/shoot");
        _asteroidHitSound = Content.Load<SoundEffect>("audio/sfx/asteroid_hit");
        _shipExplosionSound = Content.Load<SoundEffect>("audio/sfx/ship_explosion");
        _waveStartSound = Content.Load<SoundEffect>("audio/sfx/wave_start");
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (_state)
        {
            case GameState.MainMenu:
                UpdateMainMenu();
                break;
            case GameState.Playing:
                UpdatePlaying(deltaTime);
                break;
            case GameState.Paused:
                UpdatePaused();
                break;
            case GameState.GameOver:
                UpdateGameOver();
                break;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_virtualTarget);
        GraphicsDevice.Clear(new Color(4, 7, 13));

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);

        if (_state != GameState.MainMenu)
            DrawPlayfield();

        if (_state == GameState.MainMenu)
            DrawCenteredText("ASTEROIDS", "Press Enter / A to start\nEsc / Back to quit");
        else if (_state == GameState.Paused)
            DrawCenteredText("PAUSED", "Press Esc / Start to resume\nEnter / A to restart");
        else if (_state == GameState.GameOver)
            DrawCenteredText("GAME OVER", $"Score: {_score}\nWave: {_wave}\nPress Enter / A to restart\nEsc / Back for menu");

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
        _spriteBatch.Draw(_virtualTarget, _displayRectangle, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void UpdateMainMenu()
    {
        if (_input.IsExitPressed)
            Exit();
        else if (_input.IsConfirmPressed)
            StartNewGame();
    }

    private void UpdatePaused()
    {
        if (_input.IsPausePressed)
            _state = GameState.Playing;
        else if (_input.IsConfirmPressed)
            StartNewGame();
    }

    private void UpdateGameOver()
    {
        if (_input.IsExitPressed)
            _state = GameState.MainMenu;
        else if (_input.IsConfirmPressed)
            StartNewGame();
    }

    private void UpdatePlaying(float deltaTime)
    {
        if (_input.IsPausePressed)
        {
            _state = GameState.Paused;
            return;
        }

        if (_waitingForWave)
        {
            _waveTimer -= deltaTime;

            if (_waveTimer <= 0f)
                SpawnWave();
        }

        _player.Update(deltaTime, _input);
        _player.SetPosition(ScreenWrapSystem.Wrap(_player.Position, _player.CollisionRadius));

        if (_player.CanFire && _input.IsFireNewPress && _bullets.Count < GameConstants.MaxPlayerBullets)
            FireBullet();

        UpdateBullets(deltaTime);
        UpdateAsteroids(deltaTime);
        HandleCollisions();
        UpdateRespawn(deltaTime);

        if (!_waitingForWave && _asteroids.Count == 0)
            BeginNextWave();
    }

    private void StartNewGame()
    {
        _state = GameState.Playing;
        _score = 0;
        _lives = GameConstants.StartingLives;
        _wave = 0;
        _deathTimer = 0f;
        _bullets.Clear();
        _asteroids.Clear();
        _player.ResetToCenter();
        BeginNextWave();
    }

    private void BeginNextWave()
    {
        _wave++;
        _waitingForWave = true;
        _waveTimer = GameConstants.WaveTransitionDelay;
        _waveStartSound.Play(1f, 0f, 0f);
    }

    private void SpawnWave()
    {
        _waitingForWave = false;

        int asteroidCount = Math.Min(3 + _wave, 8);
        float speedMultiplier = GetWaveSpeedMultiplier();

        for (int i = 0; i < asteroidCount; i++)
        {
            Vector2 position = _asteroidSpawner.GetEdgeSpawnPosition(_player.Position);
            _asteroids.Add(_asteroidSpawner.Create(AsteroidSize.Large, position, speedMultiplier));
        }
    }

    private void FireBullet()
    {
        Vector2 spawnPosition = _player.Position + _player.Forward * 26f;
        Vector2 velocity = _player.Forward * GameConstants.BulletSpeed + _player.Velocity * 0.35f;

        _bullets.Add(new Bullet(_bulletTexture, spawnPosition, velocity));
        _player.MarkFired();
        _shootSound.Play(1f, 0f, 0f);
    }

    private void UpdateBullets(float deltaTime)
    {
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = _bullets[i];
            bullet.Update(deltaTime);
            bullet.SetPosition(ScreenWrapSystem.Wrap(bullet.Position, bullet.CollisionRadius));

            if (bullet.IsExpired)
                _bullets.RemoveAt(i);
        }
    }

    private void UpdateAsteroids(float deltaTime)
    {
        for (int i = 0; i < _asteroids.Count; i++)
        {
            Asteroid asteroid = _asteroids[i];
            asteroid.Update(deltaTime);
            asteroid.SetPosition(ScreenWrapSystem.Wrap(asteroid.Position, asteroid.Radius));
        }
    }

    private void HandleCollisions()
    {
        for (int bulletIndex = _bullets.Count - 1; bulletIndex >= 0; bulletIndex--)
        {
            Bullet bullet = _bullets[bulletIndex];

            for (int asteroidIndex = _asteroids.Count - 1; asteroidIndex >= 0; asteroidIndex--)
            {
                Asteroid asteroid = _asteroids[asteroidIndex];

                if (!CollisionSystem.CirclesOverlap(bullet.Position, bullet.CollisionRadius, asteroid.Position, asteroid.Radius))
                    continue;

                _bullets.RemoveAt(bulletIndex);
                BreakAsteroid(asteroidIndex);
                _asteroidHitSound.Play(1f, 0f, 0f);
                break;
            }
        }

        if (!_player.IsAlive || _player.IsInvulnerable)
            return;

        for (int i = 0; i < _asteroids.Count; i++)
        {
            Asteroid asteroid = _asteroids[i];

            if (CollisionSystem.CirclesOverlap(_player.Position, _player.CollisionRadius, asteroid.Position, asteroid.Radius))
            {
                KillPlayer();
                return;
            }
        }
    }

    private void BreakAsteroid(int asteroidIndex)
    {
        Asteroid asteroid = _asteroids[asteroidIndex];
        _score += asteroid.ScoreValue;
        _asteroids.RemoveAt(asteroidIndex);

        AsteroidSize? childSize = asteroid.Size switch
        {
            AsteroidSize.Large => AsteroidSize.Medium,
            AsteroidSize.Medium => AsteroidSize.Small,
            _ => null
        };

        if (childSize is null)
            return;

        float childRadius = Asteroid.GetRadius(childSize.Value);
        float speedMultiplier = GetWaveSpeedMultiplier();

        _asteroids.Add(_asteroidSpawner.CreateChild(childSize.Value, asteroid.Position, new Vector2(-childRadius, childRadius), speedMultiplier));
        _asteroids.Add(_asteroidSpawner.CreateChild(childSize.Value, asteroid.Position, new Vector2(childRadius, -childRadius), speedMultiplier));
    }

    private void KillPlayer()
    {
        _player.Kill();
        _lives--;
        _deathTimer = GameConstants.DeathDelay;
        _shipExplosionSound.Play(1f, 0f, 0f);
    }

    private void UpdateRespawn(float deltaTime)
    {
        if (_player.IsAlive)
            return;

        _deathTimer -= deltaTime;

        if (_deathTimer > 0f)
            return;

        if (_lives <= 0)
        {
            _state = GameState.GameOver;
            return;
        }

        if (IsCenterSafe())
            _player.ResetToCenter();
        else
            _deathTimer = 0.25f;
    }

    private bool IsCenterSafe()
    {
        var center = new Vector2(GameConstants.VirtualWidth * 0.5f, GameConstants.VirtualHeight * 0.5f);

        for (int i = 0; i < _asteroids.Count; i++)
        {
            Asteroid asteroid = _asteroids[i];

            if (Vector2.DistanceSquared(center, asteroid.Position) < GameConstants.SafeRespawnRadius * GameConstants.SafeRespawnRadius)
                return false;
        }

        return true;
    }

    private float GetWaveSpeedMultiplier()
    {
        return 1f + Math.Max(0, _wave - 1) * 0.08f;
    }

    private void DrawPlayfield()
    {
        DrawHud();

        for (int i = 0; i < _asteroids.Count; i++)
            _asteroids[i].Draw(_spriteBatch);

        for (int i = 0; i < _bullets.Count; i++)
            _bullets[i].Draw(_spriteBatch);

        if (_player.IsAlive && _player.IsThrusting)
            DrawThruster();

        _player.Draw(_spriteBatch);

        if (_state == GameState.Playing && _waitingForWave)
            DrawWaveText();
    }

    private void DrawHud()
    {
        string hud = $"SCORE {_score:00000}   LIVES {_lives}   WAVE {Math.Max(1, _wave)}";
        _spriteBatch.DrawString(_uiFont, hud, new Vector2(18, 16), new Color(218, 251, 255));

        if (_state == GameState.Playing)
            _spriteBatch.DrawString(_uiFont, "Esc/Start Pause", new Vector2(764, 16), new Color(92, 149, 168));
    }

    private void DrawThruster()
    {
        Vector2 position = _player.Position - _player.Forward * 24f;
        var origin = new Vector2(_thrusterTexture.Width * 0.5f, _thrusterTexture.Height * 0.5f);
        _spriteBatch.Draw(_thrusterTexture, position, null, Color.White, _player.Rotation, origin, 0.55f, SpriteEffects.None, 0f);
    }

    private void DrawWaveText()
    {
        string text = $"WAVE {_wave}";
        Vector2 size = _uiFont.MeasureString(text);
        _spriteBatch.DrawString(_uiFont, text, new Vector2((GameConstants.VirtualWidth - size.X) * 0.5f, 105f), new Color(125, 246, 255));
    }

    private void DrawCenteredText(string title, string subtitle)
    {
        Vector2 titleSize = _uiFont.MeasureString(title);
        Vector2 titlePosition = new((GameConstants.VirtualWidth - titleSize.X) * 0.5f, 275f);

        _spriteBatch.DrawString(_uiFont, title, titlePosition, new Color(223, 251, 255));

        string[] lines = subtitle.Split('\n');
        float y = 325f;

        for (int i = 0; i < lines.Length; i++)
        {
            Vector2 lineSize = _uiFont.MeasureString(lines[i]);
            _spriteBatch.DrawString(_uiFont, lines[i], new Vector2((GameConstants.VirtualWidth - lineSize.X) * 0.5f, y), new Color(125, 221, 234));
            y += 28f;
        }
    }

    private void RecalculateDisplayRectangle()
    {
        int windowWidth = Math.Max(1, Window.ClientBounds.Width);
        int windowHeight = Math.Max(1, Window.ClientBounds.Height);

        float scale = Math.Min(windowWidth / (float)GameConstants.VirtualWidth, windowHeight / (float)GameConstants.VirtualHeight);
        int width = Math.Max(1, (int)(GameConstants.VirtualWidth * scale));
        int height = Math.Max(1, (int)(GameConstants.VirtualHeight * scale));
        int x = (windowWidth - width) / 2;
        int y = (windowHeight - height) / 2;

        _displayRectangle = new Rectangle(x, y, width, height);
    }
}
