using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class OptionsScene : Scene
    {
        int _selectedIndex;
        readonly string[] _labels = { "VOLUME", "FULLSCREEN", "SCREEN SHAKE", "BACK" };
        TextComponent[] _optionTexts;
        SoundEffect _menuMove;
        SoundEffect _menuSelect;
        VirtualIntegerAxis _menuAxis;
        VirtualIntegerAxis _adjustAxis;
        VirtualButton _selectInput;
        VirtualButton _backInput;

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

            _adjustAxis = new VirtualIntegerAxis();
            _adjustAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right);
            _adjustAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D);
            _adjustAxis.AddGamePadDPadLeftRight();

            _selectInput = new VirtualButton();
            _selectInput.AddKeyboardKey(Keys.Enter);
            _selectInput.AddKeyboardKey(Keys.Space);
            _selectInput.AddGamePadButton(0, Buttons.A);

            _backInput = new VirtualButton();
            _backInput.AddKeyboardKey(Keys.Escape);
            _backInput.AddGamePadButton(0, Buttons.B);

            var font = Graphics.Instance.BitmapFont;

            var titleEntity = CreateEntity("title", new Vector2(Constants.ScreenWidth / 2f, 200));
            var titleText = titleEntity.AddComponent(new TextComponent(font, "OPTIONS", Vector2.Zero, Color.Green));
            titleText.SetHorizontalAlign(HorizontalAlign.Center);
            titleEntity.Transform.SetScale(3f);

            _optionTexts = new TextComponent[_labels.Length];
            for (int i = 0; i < _labels.Length; i++)
            {
                var entity = CreateEntity($"option_{i}", new Vector2(Constants.ScreenWidth / 2f, 340 + i * 60));
                _optionTexts[i] = entity.AddComponent(new TextComponent(font, "", Vector2.Zero, Color.Gray));
                _optionTexts[i].SetHorizontalAlign(HorizontalAlign.Center);
                entity.Transform.SetScale(3f);
            }

            RefreshLabels();
            UpdateSelection();
        }

        public override void Unload()
        {
            _menuAxis?.Deregister();
            _adjustAxis?.Deregister();
            _selectInput?.Deregister();
            _backInput?.Deregister();
        }

        public override void Update()
        {
            int dir = _menuAxis.DirectionJustPushed;
            if (dir != 0)
            {
                _selectedIndex = (_selectedIndex + dir + _labels.Length) % _labels.Length;
                UpdateSelection();
                _menuMove.Play();
            }

            int adjust = _adjustAxis.DirectionJustPushed;
            if (adjust != 0)
                AdjustOption(adjust);

            if (_selectInput.IsPressed)
            {
                if (_selectedIndex == 1)
                    ToggleFullscreen();
                else if (_selectedIndex == 2)
                    ToggleScreenShake();
                else if (_selectedIndex == 3)
                    GoBack();
            }

            if (_backInput.IsPressed)
                GoBack();

            base.Update();
        }

        void AdjustOption(int dir)
        {
            switch (_selectedIndex)
            {
                case 0:
                    var settings = Settings.Instance;
                    settings.Volume = MathHelper.Clamp(settings.Volume + dir * 0.1f, 0f, 1f);
                    SoundEffect.MasterVolume = settings.Volume;
                    _menuMove.Play();
                    RefreshLabels();
                    break;
                case 1:
                    ToggleFullscreen();
                    break;
                case 2:
                    ToggleScreenShake();
                    break;
            }
        }

        void ToggleFullscreen()
        {
            var settings = Settings.Instance;
            settings.Fullscreen = !settings.Fullscreen;
            Screen.IsFullscreen = settings.Fullscreen;
            Screen.ApplyChanges();
            _menuMove.Play();
            RefreshLabels();
        }

        void ToggleScreenShake()
        {
            var settings = Settings.Instance;
            settings.ScreenShake = !settings.ScreenShake;
            _menuMove.Play();
            RefreshLabels();
        }

        void GoBack()
        {
            _menuSelect.Play();
            Settings.Instance.Save();
            Core.StartSceneTransition(new FadeTransition(() => new MainMenuScene()));
        }

        void RefreshLabels()
        {
            var settings = Settings.Instance;
            int vol = (int)(settings.Volume * 100 + 0.5f);
            _optionTexts[0].SetText($"< VOLUME: {vol}% >");
            _optionTexts[1].SetText($"FULLSCREEN: {(settings.Fullscreen ? "ON" : "OFF")}");
            _optionTexts[2].SetText($"SCREEN SHAKE: {(settings.ScreenShake ? "ON" : "OFF")}");
            _optionTexts[3].SetText("BACK");
        }

        void UpdateSelection()
        {
            for (int i = 0; i < _optionTexts.Length; i++)
                _optionTexts[i].SetColor(i == _selectedIndex ? Color.White : Color.Gray);
        }
    }
}
