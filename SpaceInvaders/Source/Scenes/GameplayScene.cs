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
        KeyboardState _prevKb;
        GamePadState _prevGp;

        public override void Initialize()
        {
            base.Initialize();
            SetDesignResolution(GameConstants.ScreenWidth, GameConstants.ScreenHeight, SceneResolutionPolicy.ShowAll);
            ClearColor = Color.Black;

            _gameState = AddSceneComponent<GameState>();
            _waveManager = AddSceneComponent<WaveManager>();
            AddSceneComponent<BassRhythm>();

            CreatePlayer();
            CreateShields();
            CreateHud();

            _waveManager.SpawnFormation();

            _prevKb = Keyboard.GetState();
            _prevGp = GamePad.GetState(PlayerIndex.One);
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
            var shieldChunkTex = Content.LoadTexture(Assets.Sprites.Shields.ShieldChunk, true);
            float totalWidth = (GameConstants.ShieldCount - 1) * 180f;
            float startX = (GameConstants.ScreenWidth - totalWidth) / 2f;

            for (int s = 0; s < GameConstants.ShieldCount; s++)
            {
                float shieldCenterX = startX + s * 180f;
                float shieldLeft = shieldCenterX - (GameConstants.ShieldChunksX * GameConstants.ShieldChunkW) / 2f;
                float shieldTop = GameConstants.ShieldY;

                for (int cx = 0; cx < GameConstants.ShieldChunksX; cx++)
                {
                    for (int cy = 0; cy < GameConstants.ShieldChunksY; cy++)
                    {
                        float x = shieldLeft + cx * GameConstants.ShieldChunkW + GameConstants.ShieldChunkW / 2f;
                        float y = shieldTop + cy * GameConstants.ShieldChunkH + GameConstants.ShieldChunkH / 2f;

                        var chunk = CreateEntity($"shield_{s}_{cx}_{cy}", new Vector2(x, y));
                        chunk.Tag = Tags.Shield;
                        chunk.AddComponent(new SpriteRenderer(shieldChunkTex));
                        chunk.Transform.SetScale(GameConstants.ShieldScale);

                        var collider = chunk.AddComponent(new BoxCollider(GameConstants.ShieldChunkW, GameConstants.ShieldChunkH));
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

            var highEntity = CreateEntity("hud-highscore", new Vector2(GameConstants.ScreenWidth / 2f, 10));
            _highScoreText = highEntity.AddComponent(new TextComponent(font, "HI: 0", Vector2.Zero, Color.LightGray));
            _highScoreText.SetHorizontalAlign(HorizontalAlign.Center);

            var waveEntity = CreateEntity("hud-wave", new Vector2(GameConstants.ScreenWidth - 20, 10));
            _waveText = waveEntity.AddComponent(new TextComponent(font, "WAVE 1", Vector2.Zero, Color.LightGreen));
            _waveText.SetHorizontalAlign(HorizontalAlign.Right);

            var livesEntity = CreateEntity("hud-lives", new Vector2(20, GameConstants.ScreenHeight - 30));
            _livesText = livesEntity.AddComponent(new TextComponent(font, "LIVES: 3", Vector2.Zero, Color.Green));

            var gameOverEntity = CreateEntity("hud-gameover", new Vector2(GameConstants.ScreenWidth / 2f, GameConstants.ScreenHeight / 2f));
            _gameOverText = gameOverEntity.AddComponent(new TextComponent(font, "", Vector2.Zero, Color.Red));
            _gameOverText.SetHorizontalAlign(HorizontalAlign.Center);
            _gameOverText.SetVerticalAlign(VerticalAlign.Center);
        }

        public override void Update()
        {
            var kb = Keyboard.GetState();
            var gp = GamePad.GetState(PlayerIndex.One);

            if (WasPressed(kb, _prevKb, Keys.Escape) || WasPressed(gp, _prevGp, Buttons.Start))
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

            if (_gameState.IsGameOver && (WasPressed(kb, _prevKb, Keys.Enter) || WasPressed(gp, _prevGp, Buttons.A)))
            {
                Time.TimeScale = 1;
                Core.StartSceneTransition(new FadeTransition(() => new GameplayScene()));
            }

            _prevKb = kb;
            _prevGp = gp;

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

        static bool WasPressed(KeyboardState current, KeyboardState prev, Keys key)
            => current.IsKeyDown(key) && !prev.IsKeyDown(key);

        static bool WasPressed(GamePadState current, GamePadState prev, Buttons button)
            => current.IsButtonDown(button) && !prev.IsButtonDown(button);
    }
}
