using Microsoft.Xna.Framework;
using Nez;

namespace SpaceInvaders
{
    public class Game1 : Core
    {
        public Game1() : base(GameConstants.ScreenWidth, GameConstants.ScreenHeight)
        {
            Window.Title = "Space Invaders";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Core.DebugRenderEnabled = true;
            Assets.Load(Content);
            Scene = new MainMenuScene();
        }
    }
}
