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
        bool _paused;
        VirtualButton _pauseInput;
        VirtualButton _restartInput;

        public override void Initialize()
        {
            base.Initialize();
            SetDesignResolution(GameConstants.ScreenWidth, GameConstants.ScreenHeight, SceneResolutionPolicy.ShowAll);
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

            _waveManager.SpawnFormation();
        }

        void CreatePlayer()
        {
            var player = CreateEntity("player", new Vector2(GameConstants.ScreenWidth / 2f, GameConstants.PlayerY));

            var cannon = Content.LoadTexture(Assets.Sprites.Player.Cannon, true);
            player.AddComponent(new SpriteRenderer(cannon));

            var collider = player.AddComponent(new BoxCollider(60, 36));
            collider.PhysicsLayer = 1 << PhysicsLayers.Player;
            collider.CollidesWithLayers = 0;

            player.AddComponent<PlayerController>();
            player.Transform.SetScale(0.5f);
            player.Tag = Tags.Player;
        }

        void CreateShields()
        {
            var texture = Content.LoadTexture(Assets.Sprites.Shields.ShieldChunk, true);
            int count = GameConstants.ShieldCount;
            int cols = GameConstants.ShieldChunksX;
            int rows = GameConstants.ShieldChunksY;
            float chunkW = GameConstants.ShieldChunkW;
            float chunkH = GameConstants.ShieldChunkH;

            float totalWidth = (count - 1) * 180f;
            float startX = (GameConstants.ScreenWidth - totalWidth) / 2f;

            for (int s = 0; s < count; s++)
            {
                float shieldCenterX = startX + s * 180f;
                float shieldLeft = shieldCenterX - (cols * chunkW) / 2f;
                float shieldTop = GameConstants.ShieldY;

                for (int cx = 0; cx < cols; cx++)
                {
                    for (int cy = 0; cy < rows; cy++)
                    {
                        float x = shieldLeft + cx * chunkW + chunkW / 2f;
                        float y = shieldTop + cy * chunkH + chunkH / 2f;

                        var chunk = CreateEntity($"shield_{s}_{cx}_{cy}", new Vector2(x, y));
                        chunk.Tag = Tags.Shield;
                        chunk.AddComponent(new SpriteRenderer(texture));
                        chunk.Transform.SetScale(GameConstants.ShieldScale);

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

            var highEntity = CreateEntity("hud-highscore", new Vector2(GameConstants.ScreenWidth / 2f, 10));
            _highScoreText = highEntity.AddComponent(new TextComponent(font, "HI: 0", Vector2.Zero, Color.LightGray));
            _highScoreText.SetHorizontalAlign(HorizontalAlign.Center);
            highEntity.Transform.SetScale(3f);

            var waveEntity = CreateEntity("hud-wave", new Vector2(GameConstants.ScreenWidth - 20, 10));
            _waveText = waveEntity.AddComponent(new TextComponent(font, "WAVE 1", Vector2.Zero, Color.LightGreen));
            _waveText.SetHorizontalAlign(HorizontalAlign.Right);
            waveEntity.Transform.SetScale(3f);

            var livesEntity = CreateEntity("hud-lives", new Vector2(20, GameConstants.ScreenHeight - 30));
            _livesText = livesEntity.AddComponent(new TextComponent(font, "LIVES: 3", Vector2.Zero, Color.Green));
            livesEntity.Transform.SetScale(3f);

            var gameOverEntity = CreateEntity("hud-gameover", new Vector2(GameConstants.ScreenWidth / 2f, GameConstants.ScreenHeight / 2f));
            _gameOverText = gameOverEntity.AddComponent(new TextComponent(font, "", Vector2.Zero, Color.Red));
            _gameOverText.SetHorizontalAlign(HorizontalAlign.Center);
            _gameOverText.SetVerticalAlign(VerticalAlign.Center);
            gameOverEntity.Transform.SetScale(3f);
        }

        public override void Update()
        {
            if (_pauseInput.IsPressed)
            {
                if (_gameState.IsGameOver)
                {
                    Core.StartSceneTransition(new FadeTransition(() => new MainMenuScene()));
                }
                else
                {
                    _paused = !_paused;
                    Time.TimeScale = _paused ? 0 : 1;
                }
            }

            if (_gameState.IsGameOver && _restartInput.IsPressed)
            {
                Time.TimeScale = 1;
                Core.StartSceneTransition(new FadeTransition(() => new GameplayScene()));
            }

            _scoreText.SetText($"SCORE: {_gameState.Score}");
            _highScoreText.SetText($"HI: {_gameState.HighScore}");
            _livesText.SetText($"LIVES: {_gameState.Lives}");
            _waveText.SetText($"WAVE {_gameState.Wave}");

            if (_gameState.IsGameOver)
                _gameOverText.SetText("GAME OVER\nPress ENTER to restart");
            else if (_paused)
                _gameOverText.SetText("PAUSED\nPress ESC to resume");
            else
                _gameOverText.SetText("");

            base.Update();
        }
    }
}
