using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class EntityFactory
    {
        readonly Dictionary<string, Action<Scene, TmxObject>> _factories = new();

        public PlayerController Player { get; private set; }
        public Vector2 PlayerSpawn { get; private set; }

        public EntityFactory()
        {
            Register("Platform", CreatePlatform);
            Register("Mushroom", CreateMushroom);
            Register("KillVolume", CreateKillVolume);
        }

        public void Register(string type, Action<Scene, TmxObject> factory)
        {
            _factories[type] = factory;
        }

        public void Load(Scene scene, TmxObjectGroup objects)
        {
            foreach (var obj in objects.Objects)
            {
                if (obj.Type == "PlayerStart")
                {
                    PlayerSpawn = GetCenter(obj);
                    CreatePlayer(scene, PlayerSpawn);
                    break;
                }
            }

            if (Player == null)
            {
                PlayerSpawn = new Vector2(Constants.ScreenWidth / 2f, 300f);
                CreatePlayer(scene, PlayerSpawn);
            }

            foreach (var obj in objects.Objects)
            {
                if (obj.Type == "PlayerStart")
                    continue;

                if (_factories.TryGetValue(obj.Type, out var factory))
                    factory(scene, obj);
            }
        }

        public void RespawnPlayer(Scene scene)
        {
            Player.Entity.Destroy();
            CreatePlayer(scene, PlayerSpawn);
        }

        void CreatePlayer(Scene scene, Vector2 position)
        {
            var player = scene.CreateEntity("player", position);
            player.AddComponent(new PrototypeSpriteRenderer(32, 48)).SetColor(Color.Red);
            player.AddComponent(new Mover());

            var collider = player.AddComponent(new BoxCollider(-16, -24, 32, 48));
            collider.PhysicsLayer = 1 << PhysicsLayers.Player;
            collider.CollidesWithLayers = (1 << PhysicsLayers.Environment) | (1 << PhysicsLayers.Item);

            Player = player.AddComponent(new PlayerController());
        }

        void CreatePlatform(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var envLayer = 1 << PhysicsLayers.Environment;
            var envCollidesWith = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Item);

            var platform = scene.CreateEntity(obj.Name, center);
            platform.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.SaddleBrown);
            var collider = platform.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = envLayer;
            collider.CollidesWithLayers = envCollidesWith;

            if (obj.Rotation != 0)
                platform.RotationDegrees = obj.Rotation;
        }

        void CreateMushroom(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var itemCollidesWith = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Environment);

            var mushroom = scene.CreateEntity("mushroom", center);
            mushroom.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.Yellow);
            var collider = mushroom.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Item;
            collider.CollidesWithLayers = itemCollidesWith;
            collider.IsTrigger = true;
            mushroom.AddComponent(new Mushroom());
        }

        void CreateKillVolume(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var killVolume = scene.CreateEntity("killvolume", center);
            var collider = killVolume.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Player;
            collider.IsTrigger = true;
            killVolume.AddComponent(new KillVolume());
        }

        static Vector2 GetCenter(TmxObject obj)
        {
            var offset = new Vector2(obj.Width / 2f, obj.Height / 2f);

            if (obj.Rotation != 0)
            {
                var rad = MathHelper.ToRadians(obj.Rotation);
                var cos = Mathf.Cos(rad);
                var sin = Mathf.Sin(rad);
                offset = new Vector2(
                    cos * offset.X - sin * offset.Y,
                    sin * offset.X + cos * offset.Y);
            }

            return new Vector2(obj.X + offset.X, obj.Y + offset.Y);
        }
    }
}
