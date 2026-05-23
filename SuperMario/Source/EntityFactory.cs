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
            Register("PlayerStart", PlayerStart.Spawn);
            Register("Platform", Platform.Spawn);
            Register("LeftRightLift", MovingPlatform.SpawnLeftRight);
            Register("UpDownLift", MovingPlatform.SpawnUpDown);
            Register("QuestionBlock", (s, o) => QuestionBlock.Spawn(s, o, _gameState));
            Register("BrickBlock", (s, o) => BrickBlock.Spawn(s, o, _gameState));
            Register("UsedBlock", UsedBlock.Spawn);
            Register("Mushroom", (s, o) => Mushroom.Spawn(s, o, _gameState));
            Register("FireFlower", (s, o) => FireFlower.Spawn(s, o, _gameState));
            Register("OneUp", (s, o) => OneUp.Spawn(s, o, _gameState));
            Register("Coin", (s, o) => Coin.Spawn(s, o, _gameState));
            Register("Starman", (s, o) => Starman.Spawn(s, o, _gameState));
            Register("Goomba", Goomba.Spawn);
            Register("GreenKoopaTroopa", KoopaTroopa.SpawnGreen);
            Register("RedKoopaTroopa", KoopaTroopa.SpawnRed);
            Register("GreenKoopaParatroopa", KoopaParatroopa.SpawnGreen);
            Register("RedKoopaParatroopa", KoopaParatroopa.SpawnRed);
            Register("BuzzyBeetle", BuzzyBeetle.Spawn);
            Register("Spiny", Spiny.Spawn);
            Register("PiranhaPlant", PiranhaPlant.Spawn);
            Register("HammerBro", HammerBro.Spawn);
            Register("Blooper", Blooper.Spawn);
            Register("BulletBillCannon", BulletBillCannon.Spawn);
            Register("Podoboo", Podoboo.Spawn);
            Register("GoalTrigger", GoalTrigger.Spawn);
            Register("KillVolume", KillVolume.Spawn);
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
    }
}
