using Nez;
using Template.Scenes;

namespace Template
{
    public class Game1 : Core
    {
        public Game1()
            : base(Constants.ScreenWidth, Constants.ScreenHeight, false, "Template")
        {
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Scene = new MainScene();
        }
    }
}
