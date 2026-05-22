using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class MainScene : Scene
    {
        EntityFactory _factory;

        public override void Initialize()
        {
            ClearColor = Color.CornflowerBlue;

            SetDesignResolution(
                Constants.ScreenWidth,
                Constants.ScreenHeight,
                SceneResolutionPolicy.ShowAllPixelPerfect);

            AddRenderer(new DefaultRenderer());

            _factory = new EntityFactory();
            var map = Content.LoadTiledMap("Content/debug.tmx");
            _factory.Load(this, map.GetObjectGroup("entities"));

            _factory.Player.OnDied += OnPlayerDied;
        }

        void OnPlayerDied()
        {
            _factory.Player.OnDied -= OnPlayerDied;
            _factory.RespawnPlayer(this);
            _factory.Player.OnDied += OnPlayerDied;
        }
    }
}
