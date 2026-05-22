using Microsoft.Xna.Framework;
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
        HudController _hud;
        EventBus _eventBus;
        bool _paused;
        ITimer _playerRespawnTimer;
        ITimer _gameOverTimer;
        VirtualButton _pauseInput;

        public override void Initialize()
        {
            base.Initialize();
            SetDesignResolution(Constants.ScreenWidth, Constants.ScreenHeight, SceneResolutionPolicy.ShowAll);
            ClearColor = Color.Black;

            _eventBus = AddSceneComponent<EventBus>();
            _gameState = AddSceneComponent<GameState>();
            _waveManager = AddSceneComponent<WaveManager>();
            _hud = AddSceneComponent<HudController>();
            AddSceneComponent<BassRhythm>();

            _eventBus.Emitter.AddObserver(GameEvents.PlayerDied, OnPlayerDied);
            _gameState.GameOver += OnGameOver;

            _pauseInput = new VirtualButton();
            _pauseInput.AddKeyboardKey(Keys.Escape);
            _pauseInput.AddGamePadButton(0, Buttons.Start);

            Camera.Entity.AddComponent<CameraShake>();
            Camera.Entity.AddComponent<ShakeListener>();

            CreatePlayer();
            CreateShields();

            _waveManager.SpawnFormation();
        }

        public override void Unload()
        {
            _eventBus.Emitter.RemoveObserver(GameEvents.PlayerDied, OnPlayerDied);
            _gameState.GameOver -= OnGameOver;

            _playerRespawnTimer?.Stop();
            _playerRespawnTimer = null;
            _gameOverTimer?.Stop();
            _gameOverTimer = null;

            _pauseInput?.Deregister();
        }

        void CreatePlayer(bool startInvulnerable = false)
        {
            var player = CreateEntity("player", new Vector2(Constants.ScreenWidth / 2f, Constants.PlayerY));

            var cannon = Content.LoadTexture(Assets.Sprites.Player.Cannon, true);
            player.AddComponent(new SpriteRenderer(cannon));
            player.Transform.SetScale(0.5f);

            var collider = player.AddComponent(new BoxCollider(60, 36));
            Flags.SetFlagExclusive(ref collider.PhysicsLayer, PhysicsLayers.Player);
            collider.CollidesWithLayers = 0;

            player.AddComponent(new Blinker()).Enabled = false;
            player.AddComponent(new PlayerController(startInvulnerable));
        }

        void OnPlayerDied()
        {
            _gameState.LoseLife();

            if (_gameState.IsGameOver)
                return;

            _playerRespawnTimer = Core.Schedule(Constants.DeathDelay, timer =>
            {
                CreatePlayer(startInvulnerable: true);
            });
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
                        Flags.SetFlagExclusive(ref collider.PhysicsLayer, PhysicsLayers.Shield);
                        collider.CollidesWithLayers = 0;
                        collider.IsTrigger = true;

                        chunk.AddComponent<ShieldChunk>();
                    }
                }
            }
        }

        public override void Update()
        {
            if (!_gameState.IsGameOver && _pauseInput.IsPressed)
                TogglePause();

            base.Update();
        }

        void TogglePause()
        {
            _paused = !_paused;
            Time.TimeScale = _paused ? 0 : 1;

            if (_paused)
                _hud.ShowPause();
            else
                _hud.HidePause();
        }

        void OnGameOver()
        {
            _gameOverTimer = Core.Schedule(2f, _ =>
            {
                Time.TimeScale = 1;
                bool isNew = _gameState.Score >= _gameState.HighScore && _gameState.Score > 0;
                Core.StartSceneTransition(new FadeTransition(() =>
                    new GameOverScene(_gameState.Score, _gameState.HighScore, isNew)));
            });
        }
    }
}
