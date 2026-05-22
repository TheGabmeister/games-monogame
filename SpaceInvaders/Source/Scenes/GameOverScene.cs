using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class GameOverScene : Scene
    {
        readonly int _finalScore;
        readonly int _highScore;
        readonly bool _isNewHighScore;

        int _selectedIndex;
        readonly string[] _options = { "Restart", "Main Menu" };
        TextComponent[] _optionTexts;
        SoundEffect _menuMove;
        SoundEffect _menuSelect;
        VirtualIntegerAxis _menuAxis;
        VirtualButton _selectInput;

        public GameOverScene(int finalScore, int highScore, bool isNewHighScore)
        {
            _finalScore = finalScore;
            _highScore = highScore;
            _isNewHighScore = isNewHighScore;
        }

        public override void Initialize()
        {
            base.Initialize();
            SetDesignResolution(Constants.ScreenWidth, Constants.ScreenHeight, SceneResolutionPolicy.ShowAll);
            ClearColor = Color.Black;

            _menuMove = Content.LoadSoundEffect(Assets.Audio.Sfx.MenuMove);
            _menuSelect = Content.LoadSoundEffect(Assets.Audio.Sfx.MenuSelect);

            _menuAxis = new VirtualIntegerAxis();
            _menuAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Up, Keys.Down);
            _menuAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.W, Keys.S);
            _menuAxis.AddGamePadDPadUpDown();

            _selectInput = new VirtualButton();
            _selectInput.AddKeyboardKey(Keys.Enter);
            _selectInput.AddKeyboardKey(Keys.Space);
            _selectInput.AddGamePadButton(0, Buttons.A);

            var font = Graphics.Instance.BitmapFont;

            var titleEntity = CreateEntity("title", new Vector2(Constants.ScreenWidth / 2f, 160));
            var titleText = titleEntity.AddComponent(new TextComponent(font, "GAME OVER", Vector2.Zero, Color.Red));
            titleText.SetHorizontalAlign(HorizontalAlign.Center);
            titleEntity.Transform.SetScale(4f);

            var scoreEntity = CreateEntity("score", new Vector2(Constants.ScreenWidth / 2f, 280));
            var scoreText = scoreEntity.AddComponent(new TextComponent(font, $"SCORE: {_finalScore}", Vector2.Zero, Color.White));
            scoreText.SetHorizontalAlign(HorizontalAlign.Center);
            scoreEntity.Transform.SetScale(3f);

            var highScoreEntity = CreateEntity("highscore", new Vector2(Constants.ScreenWidth / 2f, 340));
            var highScoreText = highScoreEntity.AddComponent(new TextComponent(font, $"HI: {_highScore}", Vector2.Zero, Color.LightGray));
            highScoreText.SetHorizontalAlign(HorizontalAlign.Center);
            highScoreEntity.Transform.SetScale(3f);

            if (_isNewHighScore)
            {
                var newRecordEntity = CreateEntity("new-record", new Vector2(Constants.ScreenWidth / 2f, 400));
                var newRecordText = newRecordEntity.AddComponent(new TextComponent(font, "NEW HIGH SCORE!", Vector2.Zero, Color.Yellow));
                newRecordText.SetHorizontalAlign(HorizontalAlign.Center);
                newRecordEntity.Transform.SetScale(3f);
            }

            float menuStartY = _isNewHighScore ? 480 : 440;
            _optionTexts = new TextComponent[_options.Length];
            for (int i = 0; i < _options.Length; i++)
            {
                var entity = CreateEntity($"option_{i}", new Vector2(Constants.ScreenWidth / 2f, menuStartY + i * 50));
                _optionTexts[i] = entity.AddComponent(new TextComponent(font, _options[i], Vector2.Zero, Color.Gray));
                _optionTexts[i].SetHorizontalAlign(HorizontalAlign.Center);
                entity.Transform.SetScale(3f);
            }

            UpdateSelection();
        }

        public override void Unload()
        {
            _menuAxis?.Deregister();
            _selectInput?.Deregister();
        }

        public override void Update()
        {
            int dir = _menuAxis.DirectionJustPushed;
            if (dir != 0)
            {
                _selectedIndex = (_selectedIndex + dir + _options.Length) % _options.Length;
                UpdateSelection();
                _menuMove.Play();
            }

            if (_selectInput.IsPressed)
            {
                _menuSelect.Play();
                if (_selectedIndex == 0)
                    Core.StartSceneTransition(new FadeTransition(() => new GameplayScene()));
                else
                    Core.StartSceneTransition(new FadeTransition(() => new MainMenuScene()));
            }

            base.Update();
        }

        void UpdateSelection()
        {
            for (int i = 0; i < _optionTexts.Length; i++)
                _optionTexts[i].SetColor(i == _selectedIndex ? Color.White : Color.Gray);
        }
    }
}
