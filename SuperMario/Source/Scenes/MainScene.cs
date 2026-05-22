using Microsoft.Xna.Framework;
using Nez;
using SuperMario.Components;

namespace SuperMario.Scenes
{
    public class MainScene : Scene
    {
        public override void Initialize()
        {
            ClearColor = Color.CornflowerBlue;

            SetDesignResolution(
                Constants.ScreenWidth,
                Constants.ScreenHeight,
                SceneResolutionPolicy.ShowAllPixelPerfect);

            AddRenderer(new DefaultRenderer());

            CreatePlayer();
            CreateLevel();

        }

        private void CreatePlayer()
        {
            var player = CreateEntity("player", new Vector2(Constants.ScreenWidth / 2f, 500f));
            player.AddComponent(new PrototypeSpriteRenderer(32, 48)).SetColor(Color.Red);
            player.AddComponent(new BoxCollider(-16, -24, 32, 48));
            player.AddComponent(new Mover());
            player.AddComponent(new PlayerController());
        }

        private void CreateLevel()
        {
            var groundY = Constants.ScreenHeight - 40;
            var ground = CreateEntity("ground", new Vector2(Constants.ScreenWidth / 2f, groundY));
            ground.AddComponent(new PrototypeSpriteRenderer(Constants.ScreenWidth, 80)).SetColor(Color.SaddleBrown);
            ground.AddComponent(new BoxCollider(-Constants.ScreenWidth / 2f, -40, Constants.ScreenWidth, 80));

            var platform1 = CreateEntity("platform1", new Vector2(300f, 520f));
            platform1.AddComponent(new PrototypeSpriteRenderer(160, 20)).SetColor(Color.ForestGreen);
            platform1.AddComponent(new BoxCollider(-80, -10, 160, 20));

            var platform2 = CreateEntity("platform2", new Vector2(660f, 420f));
            platform2.AddComponent(new PrototypeSpriteRenderer(160, 20)).SetColor(Color.ForestGreen);
            platform2.AddComponent(new BoxCollider(-80, -10, 160, 20));

            var platform3 = CreateEntity("platform3", new Vector2(480f, 320f));
            platform3.AddComponent(new PrototypeSpriteRenderer(160, 20)).SetColor(Color.ForestGreen);
            platform3.AddComponent(new BoxCollider(-80, -10, 160, 20));
        }
    }
}
