using Microsoft.Xna.Framework.Audio;
using Nez;

namespace SpaceInvaders
{
    public class Game1 : Core
    {
        public Game1() : base(Constants.ScreenWidth, Constants.ScreenHeight)
        {
            Window.Title = "Space Invaders";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            DebugRenderEnabled = false;

            var settings = Settings.Instance;
            SoundEffect.MasterVolume = settings.Volume;
            Screen.HardwareModeSwitch = false;
            if (settings.Fullscreen)
            {
                Screen.IsFullscreen = true;
                Screen.ApplyChanges();
            }

            Scene = new MainMenuScene();
        }
    }
}
