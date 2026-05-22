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

            var map = Content.LoadTiledMap("Content/debug.tmx");
            var objects = map.GetObjectGroup("entities");

            foreach (var obj in objects.Objects)
            {
                var center = new Vector2(obj.X + obj.Width / 2f, obj.Y + obj.Height / 2f);

                switch (obj.Type)
                {
                    case "PlayerStart":
                        CreatePlayer(center);
                        break;
                    case "Platform":
                        CreatePlatform(obj.Name, center, obj.Width, obj.Height);
                        break;
                    case "Mushroom":
                        CreateMushroom(center, obj.Width, obj.Height);
                        break;
                }
            }

            if (_playerController == null)
                CreatePlayer(new Vector2(Constants.ScreenWidth / 2f, 300f));
        }

        private void CreatePlayer(Vector2 position)
        {
            var player = CreateEntity("player", position);
            player.AddComponent(new PrototypeSpriteRenderer(32, 48)).SetColor(Color.Red);
            player.AddComponent(new Mover());

            var collider = player.AddComponent(new BoxCollider(-16, -24, 32, 48));
            collider.PhysicsLayer = 1 << PhysicsLayers.Player;
            collider.CollidesWithLayers = (1 << PhysicsLayers.Environment) | (1 << PhysicsLayers.Item);

            _playerController = player.AddComponent(new PlayerController());
        }

        private void CreatePlatform(string name, Vector2 position, float width, float height)
        {
            var envLayer = 1 << PhysicsLayers.Environment;
            var envCollidesWith = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Item);

            var platform = CreateEntity(name, position);
            platform.AddComponent(new PrototypeSpriteRenderer(width, height)).SetColor(Color.SaddleBrown);
            var collider = platform.AddComponent(new BoxCollider(-width / 2f, -height / 2f, width, height));
            collider.PhysicsLayer = envLayer;
            collider.CollidesWithLayers = envCollidesWith;
        }

        private void CreateMushroom(Vector2 position, float width, float height)
        {
            var itemCollidesWith = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Environment);

            var mushroom = CreateEntity("mushroom", position);
            mushroom.AddComponent(new PrototypeSpriteRenderer(width, height)).SetColor(Color.Yellow);
            var collider = mushroom.AddComponent(new BoxCollider(-width / 2f, -height / 2f, width, height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Item;
            collider.CollidesWithLayers = itemCollidesWith;
            collider.IsTrigger = true;
            mushroom.AddComponent(new Mushroom(_playerController));
        }
    }
}
