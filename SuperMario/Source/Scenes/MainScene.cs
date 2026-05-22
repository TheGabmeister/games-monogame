using Microsoft.Xna.Framework;
using Nez;


namespace SuperMario
{
    public class MainScene : Scene
    {
        PlayerController _playerController;

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
            player.AddComponent(new Mover());

            var collider = player.AddComponent(new BoxCollider(-16, -24, 32, 48));
            collider.PhysicsLayer = 1 << PhysicsLayers.Player;
            collider.CollidesWithLayers = (1 << PhysicsLayers.Environment) | (1 << PhysicsLayers.Item);
            
            _playerController = player.AddComponent(new PlayerController());
        }

        private void CreateLevel()
        {
            var envLayer = 1 << PhysicsLayers.Environment;
            var envCollidesWith = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Item);

            var groundY = Constants.ScreenHeight - 40;
            var ground = CreateEntity("ground", new Vector2(Constants.ScreenWidth / 2f, groundY));
            ground.AddComponent(new PrototypeSpriteRenderer(Constants.ScreenWidth, 80)).SetColor(Color.SaddleBrown);
            var gc = ground.AddComponent(new BoxCollider(-Constants.ScreenWidth / 2f, -40, Constants.ScreenWidth, 80));
            gc.PhysicsLayer = envLayer;
            gc.CollidesWithLayers = envCollidesWith;

            CreatePlatform("platform1", new Vector2(300f, 520f), envLayer, envCollidesWith);
            CreatePlatform("platform2", new Vector2(660f, 420f), envLayer, envCollidesWith);
            CreatePlatform("platform3", new Vector2(480f, 320f), envLayer, envCollidesWith);

            CreateMushroom(new Vector2(300f, 505f));
            CreateMushroom(new Vector2(660f, 405f));
        }

        private void CreatePlatform(string name, Vector2 position, int layer, int collidesWith)
        {
            var platform = CreateEntity(name, position);
            platform.AddComponent(new PrototypeSpriteRenderer(160, 20)).SetColor(Color.ForestGreen);
            var collider = platform.AddComponent(new BoxCollider(-80, -10, 160, 20));
            collider.PhysicsLayer = layer;
            collider.CollidesWithLayers = collidesWith;
        }

        private void CreateMushroom(Vector2 position)
        {
            var itemCollidesWith = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Environment);

            var mushroom = CreateEntity("mushroom", position);
            mushroom.AddComponent(new PrototypeSpriteRenderer(20, 20)).SetColor(Color.Yellow);
            var collider = mushroom.AddComponent(new BoxCollider(-10, -10, 20, 20));
            collider.PhysicsLayer = 1 << PhysicsLayers.Item;
            collider.CollidesWithLayers = itemCollidesWith;
            collider.IsTrigger = true;
            mushroom.AddComponent(new Mushroom(_playerController));
        }
    }
}
