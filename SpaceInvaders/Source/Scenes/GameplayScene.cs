using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Systems;

namespace SpaceInvaders
{
    public class GameplayScene : Scene
    {
        GameState _gameState;
        WaveManager _waveManager;
        TextComponent _scoreText;
        TextComponent _highScoreText;
        TextComponent _livesText;
        TextComponent _waveText;
        TextComponent _gameOverText;
        PlayerController _playerController;
        bool _paused;
        ITimer _playerRespawnTimer;
        VirtualButton _pauseInput;
        VirtualButton _restartInput;

        public override void Initialize()
        {
            base.Initialize();
            SetDesignResolution(Constants.ScreenWidth, Constants.ScreenHeight, SceneResolutionPolicy.ShowAll);
            ClearColor = Color.Black;

            _gameState = AddSceneComponent<GameState>();
            _waveManager = AddSceneComponent<WaveManager>();
            AddSceneComponent<BassRhythm>();

            _pauseInput = new VirtualButton();
            _pauseInput.AddKeyboardKey(Keys.Escape);
            _pauseInput.AddGamePadButton(0, Buttons.Start);

            _restartInput = new VirtualButton();
            _restartInput.AddKeyboardKey(Keys.Enter);
            _restartInput.AddGamePadButton(0, Buttons.A);

            CreatePlayer();
            CreateShields();
            CreateHud();
            HookGameStateEvents();
            RefreshHud();

            _waveManager.SpawnFormation();
        }

        public override void Unload()
        {
            _playerRespawnTimer?.Stop();
            _playerRespawnTimer = null;

            UnhookGameStateEvents();
            _pauseInput?.Deregister();
            _restartInput?.Deregister();
        }

        void CreatePlayer(bool startInvulnerable = false)
        {
            var player = CreateEntity("player", new Vector2(Constants.ScreenWidth / 2f, Constants.PlayerY));

            var cannon = Content.LoadTexture(Assets.Sprites.Player.Cannon, true);
            player.AddComponent(new SpriteRenderer(cannon));
            player.Transform.SetScale(0.5f);

            var collider = player.AddComponent(new BoxCollider(60, 36));
            collider.PhysicsLayer = 1 << PhysicsLayers.Player;
            collider.CollidesWithLayers = 0;

            player.AddComponent(new Blinker()).Enabled = false;

            var controller = player.AddComponent(new PlayerController(startInvulnerable));
            controller.Died += OnPlayerDied;

            _playerController = controller;
        }

        void OnPlayerDied()
        {
            _playerController.Died -= OnPlayerDied;
            _gameState.LoseLife();

            if (_gameState.IsGameOver)
                return;

            _playerRespawnTimer = Core.Schedule(Constants.DeathDelay, timer =>
            {
                CreatePlayer(startInvulnerable: true);
            });
        }

        void OnGameOver()
        {
            _gameOverText.SetText("GAME OVER\nPress ENTER to restart");
            _gameOverText.Enabled = true;
        }

        void HookGameStateEvents()
        {
            _gameState.ScoreChanged += OnScoreChanged;
            _gameState.HighScoreChanged += OnHighScoreChanged;
            _gameState.LivesChanged += OnLivesChanged;
            _gameState.WaveChanged += OnWaveChanged;
            _gameState.GameOver += OnGameOver;
        }

        void UnhookGameStateEvents()
        {
            if (_gameState == null)
                return;

            _gameState.ScoreChanged -= OnScoreChanged;
            _gameState.HighScoreChanged -= OnHighScoreChanged;
            _gameState.LivesChanged -= OnLivesChanged;
            _gameState.WaveChanged -= OnWaveChanged;
            _gameState.GameOver -= OnGameOver;
        }

        void RefreshHud()
        {
            OnScoreChanged(_gameState.Score);
            OnHighScoreChanged(_gameState.HighScore);
            OnLivesChanged(_gameState.Lives);
            OnWaveChanged(_gameState.Wave);
        }

        void OnScoreChanged(int score)
        {
            _scoreText.SetText($"SCORE: {score}");
        }

        void OnHighScoreChanged(int highScore)
        {
            _highScoreText.SetText($"HI: {highScore}");
        }

        void OnLivesChanged(int lives)
        {
            _livesText.SetText($"LIVES: {lives}");
        }

        void OnWaveChanged(int wave)
        {
            _waveText.SetText($"WAVE {wave}");
        }

        void CreateShields()
        {
            var texture = Content.LoadTexture(Assets.Sprites.Shields.ShieldChunk, true);
            int count = Constants.ShieldCount;
            int cols = Constants.ShieldChunksX;
            int rows = Constants.ShieldChunksY;
            float chunkW = Constants.ShieldChunkW;
            float chunkH = Constants.ShieldChunkH;

            float totalWidth = (count - 1) * 180f;
            float startX = (Constants.ScreenWidth - totalWidth) / 2f;

            for (int s = 0; s < count; s++)
            {
                float shieldCenterX = startX + s * 180f;
                float shieldLeft = shieldCenterX - (cols * chunkW) / 2f;
                float shieldTop = Constants.ShieldY;

                for (int cx = 0; cx < cols; cx++)
                {
                    for (int cy = 0; cy < rows; cy++)
                    {
                        float x = shieldLeft + cx * chunkW + chunkW / 2f;
                        float y = shieldTop + cy * chunkH + chunkH / 2f;

                        var chunk = CreateEntity($"shield_{s}_{cx}_{cy}", new Vector2(x, y));
                        chunk.AddComponent(new SpriteRenderer(texture));
                        chunk.Transform.SetScale(Constants.ShieldScale);

                        var collider = chunk.AddComponent(new BoxCollider(chunkW, chunkH));
                        collider.PhysicsLayer = 1 << PhysicsLayers.Shield;
                        collider.CollidesWithLayers = 0;
                        collider.IsTrigger = true;

                        chunk.AddComponent<ShieldChunk>();
                    }
                }
            }
        }

        void CreateHud()
        {
            var font = Graphics.Instance.BitmapFont;

            var scoreEntity = CreateEntity("hud-score", new Vector2(20, 10));
            _scoreText = scoreEntity.AddComponent(new TextComponent(font, "SCORE: 0", Vector2.Zero, Color.White));
            scoreEntity.Transform.SetScale(3f);

            var highEntity = CreateEntity("hud-highscore", new Vector2(Constants.ScreenWidth / 2f, 10));
            _highScoreText = highEntity.AddComponent(new TextComponent(font, "HI: 0", Vector2.Zero, Color.LightGray));
            _highScoreText.SetHorizontalAlign(HorizontalAlign.Center);
            highEntity.Transform.SetScale(3f);

            var waveEntity = CreateEntity("hud-wave", new Vector2(Constants.ScreenWidth - 20, 10));
            _waveText = waveEntity.AddComponent(new TextComponent(font, "WAVE 1", Vector2.Zero, Color.LightGreen));
            _waveText.SetHorizontalAlign(HorizontalAlign.Right);
            waveEntity.Transform.SetScale(3f);

            var livesEntity = CreateEntity("hud-lives", new Vector2(20, Constants.ScreenHeight - 30));
            _livesText = livesEntity.AddComponent(new TextComponent(font, "LIVES: 3", Vector2.Zero, Color.Green));
            livesEntity.Transform.SetScale(3f);

            var gameOverEntity = CreateEntity("hud-gameover", new Vector2(Constants.ScreenWidth / 2f, Constants.ScreenHeight / 2f));
            _gameOverText = gameOverEntity.AddComponent(new TextComponent(font, "GAME OVER\nPress ENTER to restart", Vector2.Zero, Color.Red));
            _gameOverText.SetHorizontalAlign(HorizontalAlign.Center);
            _gameOverText.SetVerticalAlign(VerticalAlign.Center);
            _gameOverText.Enabled = false;
            gameOverEntity.Transform.SetScale(3f);
        }

        public override void Update()
        {
            if (_pauseInput.IsPressed)
                HandlePausePressed();

            if (_gameState.IsGameOver && _restartInput.IsPressed)
                RestartGame();

            base.Update();
        }

        void HandlePausePressed()
        {
            if (_gameState.IsGameOver)
            {
                Core.StartSceneTransition(new FadeTransition(() => new MainMenuScene()));
                return;
            }

            _paused = !_paused;
            Time.TimeScale = _paused ? 0 : 1;

            if (_paused)
            {
                _gameOverText.SetText("PAUSED\nPress ESC to resume");
                _gameOverText.Enabled = true;
            }
            else
            {
                _gameOverText.SetText("");
                _gameOverText.Enabled = false;
            }
        }

        void RestartGame()
        {
            Time.TimeScale = 1;
            Core.StartSceneTransition(new FadeTransition(() => new GameplayScene()));
        }
    }
}
