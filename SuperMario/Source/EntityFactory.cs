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
        readonly GameState _gameState;

        public EntityFactory(GameState gameState)
        {
            _gameState = gameState;
            Register("PlayerStart", CreatePlayerStart);
            Register("Platform", CreatePlatform);
            Register("Mushroom", CreateMushroom);
            Register("OneUp", CreateOneUp);
            Register("GoalTrigger", CreateGoalTrigger);
            Register("KillVolume", CreateKillVolume);
        }

        public void Register(string type, Action<Scene, TmxObject> factory)
        {
            _factories[type] = factory;
        }

        public void Spawn(Scene scene, TmxObject obj)
        {
            if (_factories.TryGetValue(obj.Type, out var factory))
                factory(scene, obj);
        }

        void CreatePlayerStart(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            scene.CreateEntity("playerstart", center)
                .AddComponent(new PlayerStart());
        }

        public PlayerController CreatePlayer(Scene scene, Vector2 position)
        {
            var player = scene.CreateEntity("player", position);
            player.AddComponent(new PrototypeSpriteRenderer(32, 48)).SetColor(Color.Red);
            player.AddComponent(new Mover());

            var collider = player.AddComponent(new BoxCollider(-16, -24, 32, 48));
            collider.PhysicsLayer = 1 << PhysicsLayers.Player;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            return player.AddComponent(new PlayerController());
        }

        public static Vector2 GetCenter(TmxObject obj)
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
            var w = obj.Width;
            var h = obj.Height;

            var mushroom = scene.CreateEntity("mushroom", center);
            mushroom.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Yellow);
            mushroom.AddComponent(new Mover());
            mushroom.AddComponent(new GravityBody());

            var body = mushroom.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Item;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var pickup = mushroom.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.Item;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            mushroom.AddComponent(new Mushroom());
        }

        void CreateOneUp(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var oneUp = scene.CreateEntity("oneup", center);
            oneUp.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Green);
            oneUp.AddComponent(new Mover());
            oneUp.AddComponent(new GravityBody());

            var body = oneUp.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Item;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var pickup = oneUp.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.Item;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            oneUp.AddComponent(new OneUp(_gameState));
        }

        void CreateGoalTrigger(Scene scene, TmxObject obj)
        {
            var center = GetCenter(obj);
            var goal = scene.CreateEntity("goaltrigger", center);
            goal.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.Gold);

            var collider = goal.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Player;
            collider.IsTrigger = true;

            goal.AddComponent(new GoalTrigger());
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
    }
}
