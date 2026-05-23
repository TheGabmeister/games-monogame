using Nez;

namespace SuperMario
{
    public class Game1 : Core
    {
        public Game1() : base(
            Constants.ScreenWidth, 
            Constants.ScreenHeight, 
            false, 
            "SuperMario"
            )
        {
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            DebugRenderEnabled = true;

            RegisterGlobalManager(new MusicManager());
            RegisterGlobalManager(new SfxManager());
            RegisterGlobalManager(new GameManager());
        }
    }
}
