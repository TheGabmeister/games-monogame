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
            Scene = new MainMenuScene();
        }
    }
}
