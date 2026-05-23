using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class FireFlower : Component, ITriggerListener
    {
        const int PickupValue = 1000;

        readonly GameState _gameState;
        bool _collected;

        public FireFlower(GameState gameState)
        {
            _gameState = gameState;
        }

        public static void Spawn(Scene scene, TmxObject obj, GameState gameState)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var fireFlower = scene.CreateEntity("fireflower", center);
            fireFlower.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.OrangeRed);
            fireFlower.AddComponent(new Mover());
            fireFlower.AddComponent(new GravityBody());

            var body = fireFlower.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Item;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var pickup = fireFlower.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.Item;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            fireFlower.AddComponent(new FireFlower(gameState));
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (_collected)
                return;

            var player = other.Entity.GetComponent<PlayerController>();
            if (player == null)
                return;

            _collected = true;
            _gameState.AddScore(PickupValue);
            ScorePopup.Spawn(Entity.Scene, Entity.Position, PickupValue);
            Audio.PlaySfx(Assets.Sfx.PickupMushroom);
            player.ApplyFireFlower();
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
