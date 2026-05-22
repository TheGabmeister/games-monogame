using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class HudController : SceneComponent
    {
        GameState _gameState;
        TextComponent _scoreText;
        TextComponent _highScoreText;
        TextComponent _livesText;
        TextComponent _waveText;
        TextComponent _pauseText;

        public override void OnEnabled()
        {
            _gameState = Scene.GetSceneComponent<GameState>();

            CreateHud();
            HookGameStateEvents();
            RefreshHud();
        }

        public override void OnDisabled()
        {
            UnhookGameStateEvents();
        }

        public override void OnRemovedFromScene()
        {
            UnhookGameStateEvents();
        }

        public void ShowPause()
        {
            _pauseText.Enabled = true;
        }

        public void HidePause()
        {
            _pauseText.Enabled = false;
        }

        void CreateHud()
        {
            var font = Graphics.Instance.BitmapFont;

            var scoreEntity = Scene.CreateEntity("hud-score", new Vector2(20, 10));
            _scoreText = scoreEntity.AddComponent(new TextComponent(font, "SCORE: 0", Vector2.Zero, Color.White));
            scoreEntity.Transform.SetScale(3f);

            var highEntity = Scene.CreateEntity("hud-highscore", new Vector2(Constants.ScreenWidth / 2f, 10));
            _highScoreText = highEntity.AddComponent(new TextComponent(font, "HI: 0", Vector2.Zero, Color.LightGray));
            _highScoreText.SetHorizontalAlign(HorizontalAlign.Center);
            highEntity.Transform.SetScale(3f);

            var waveEntity = Scene.CreateEntity("hud-wave", new Vector2(Constants.ScreenWidth - 20, 10));
            _waveText = waveEntity.AddComponent(new TextComponent(font, "WAVE 1", Vector2.Zero, Color.LightGreen));
            _waveText.SetHorizontalAlign(HorizontalAlign.Right);
            waveEntity.Transform.SetScale(3f);

            var livesEntity = Scene.CreateEntity("hud-lives", new Vector2(20, Constants.ScreenHeight - 30));
            _livesText = livesEntity.AddComponent(new TextComponent(font, "LIVES: 3", Vector2.Zero, Color.Green));
            livesEntity.Transform.SetScale(3f);

            var pauseEntity = Scene.CreateEntity("hud-pause", new Vector2(Constants.ScreenWidth / 2f, Constants.ScreenHeight / 2f));
            _pauseText = pauseEntity.AddComponent(new TextComponent(font, "PAUSED\nPress ESC to resume", Vector2.Zero, Color.White));
            _pauseText.SetHorizontalAlign(HorizontalAlign.Center);
            _pauseText.SetVerticalAlign(VerticalAlign.Center);
            _pauseText.Enabled = false;
            pauseEntity.Transform.SetScale(3f);
        }

        void HookGameStateEvents()
        {
            _gameState.ScoreChanged += OnScoreChanged;
            _gameState.HighScoreChanged += OnHighScoreChanged;
            _gameState.LivesChanged += OnLivesChanged;
            _gameState.WaveChanged += OnWaveChanged;
        }

        void UnhookGameStateEvents()
        {
            if (_gameState == null)
                return;

            _gameState.ScoreChanged -= OnScoreChanged;
            _gameState.HighScoreChanged -= OnHighScoreChanged;
            _gameState.LivesChanged -= OnLivesChanged;
            _gameState.WaveChanged -= OnWaveChanged;
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
    }
}
