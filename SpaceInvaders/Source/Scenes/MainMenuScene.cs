using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class MainMenuScene : Scene
    {
        int _selectedIndex;
        readonly string[] _options = { "Start Game", "Quit" };
        TextComponent[] _optionTexts;
        TextComponent _titleText;
        SoundEffect _menuMove;
        SoundEffect _menuSelect;
        VirtualIntegerAxis _menuAxis;
        VirtualButton _selectInput;

        public override void Initialize()
        {
            base.Initialize();
            SetDesignResolution(GameConstants.ScreenWidth, GameConstants.ScreenHeight, SceneResolutionPolicy.ShowAll);
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

            var titleEntity = CreateEntity("title", new Vector2(GameConstants.ScreenWidth / 2f, 200));
            _titleText = titleEntity.AddComponent(new TextComponent(font, "SPACE INVADERS", Vector2.Zero, Color.Green));
            _titleText.SetHorizontalAlign(HorizontalAlign.Center);
            titleEntity.Transform.SetScale(3f);

            _optionTexts = new TextComponent[_options.Length];
            for (int i = 0; i < _options.Length; i++)
            {
                var entity = CreateEntity($"option_{i}", new Vector2(GameConstants.ScreenWidth / 2f, 360 + i * 50));
                _optionTexts[i] = entity.AddComponent(new TextComponent(font, _options[i], Vector2.Zero, Color.Gray));
                _optionTexts[i].SetHorizontalAlign(HorizontalAlign.Center);
                entity.Transform.SetScale(3f);
            }

            UpdateSelection();
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
                else if (_selectedIndex == 1)
                    Core.Exit();
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
