using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class Mushroom : Component, ITriggerListener
    {
        const int PickupValue = 1000;

        readonly GameState _gameState;
        bool _collected;

        public Mushroom(GameState gameState)
        {
            _gameState = gameState;
        }

        public static void Spawn(Scene scene, TmxObject obj, GameState gameState)
        {
            var center = EntityFactory.GetCenter(obj);
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

            mushroom.AddComponent(new Mushroom(gameState));
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (_collected)
                return;

            var player = other.Entity.GetComponent<PlayerController>();
            if (player != null)
            {
                _collected = true;
                _gameState.AddScore(PickupValue);
                ScorePopup.Spawn(Entity.Scene, Entity.Position, PickupValue);
                Audio.PlaySfx(Assets.Sfx.PickupMushroom);
                player.ApplyMushroom();
                Entity.Destroy();
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
