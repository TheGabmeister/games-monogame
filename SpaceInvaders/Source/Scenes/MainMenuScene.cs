using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace SpaceInvaders
{
    public class MainMenuScene : Scene
    {
        int _selectedIndex;
        readonly string[] _options = { "Start Game", "Quit" };
        TextComponent[] _optionTexts;
        TextComponent _titleText;
        KeyboardState _prevKb;
        GamePadState _prevGp;

        public override void Initialize()
        {
            base.Initialize();
            SetDesignResolution(GameConstants.ScreenWidth, GameConstants.ScreenHeight, SceneResolutionPolicy.ShowAll);
            ClearColor = Color.Black;
            Assets.Load(Content);

            var font = Graphics.Instance.BitmapFont;

            var titleEntity = CreateEntity("title", new Vector2(GameConstants.ScreenWidth / 2f, 200));
            _titleText = titleEntity.AddComponent(new TextComponent(font, "SPACE INVADERS", Vector2.Zero, Color.Green));
            _titleText.SetHorizontalAlign(HorizontalAlign.Center);

            _optionTexts = new TextComponent[_options.Length];
            for (int i = 0; i < _options.Length; i++)
            {
                var entity = CreateEntity($"option_{i}", new Vector2(GameConstants.ScreenWidth / 2f, 360 + i * 50));
                _optionTexts[i] = entity.AddComponent(new TextComponent(font, _options[i], Vector2.Zero, Color.Gray));
                _optionTexts[i].SetHorizontalAlign(HorizontalAlign.Center);
            }

            _prevKb = Keyboard.GetState();
            _prevGp = GamePad.GetState(PlayerIndex.One);
            UpdateSelection();
        }

        public override void Update()
        {
            var kb = Keyboard.GetState();
            var gp = GamePad.GetState(PlayerIndex.One);

            if (WasPressed(kb, _prevKb, Keys.Up) || WasPressed(kb, _prevKb, Keys.W) || WasPressed(gp, _prevGp, Buttons.DPadUp))
            {
                _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;
                UpdateSelection();
                Assets.MenuMove.Play();
            }
            else if (WasPressed(kb, _prevKb, Keys.Down) || WasPressed(kb, _prevKb, Keys.S) || WasPressed(gp, _prevGp, Buttons.DPadDown))
            {
                _selectedIndex = (_selectedIndex + 1) % _options.Length;
                UpdateSelection();
                Assets.MenuMove.Play();
            }

            if (WasPressed(kb, _prevKb, Keys.Enter) || WasPressed(kb, _prevKb, Keys.Space) || WasPressed(gp, _prevGp, Buttons.A))
            {
                Assets.MenuSelect.Play();
                if (_selectedIndex == 0)
                    Core.StartSceneTransition(new FadeTransition(() => new GameplayScene()));
                else if (_selectedIndex == 1)
                    Core.Exit();
            }

            _prevKb = kb;
            _prevGp = gp;

            base.Update();
        }

        void UpdateSelection()
        {
            for (int i = 0; i < _optionTexts.Length; i++)
                _optionTexts[i].SetColor(i == _selectedIndex ? Color.White : Color.Gray);
        }

        static bool WasPressed(KeyboardState current, KeyboardState prev, Keys key)
            => current.IsKeyDown(key) && !prev.IsKeyDown(key);

        static bool WasPressed(GamePadState current, GamePadState prev, Buttons button)
            => current.IsButtonDown(button) && !prev.IsButtonDown(button);
    }
}
