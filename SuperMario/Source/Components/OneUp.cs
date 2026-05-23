using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class OneUp : Component, ITriggerListener
    {
        readonly GameState _gameState;
        bool _collected;

        public OneUp(GameState gameState)
        {
            _gameState = gameState;
        }

        public static void Spawn(Scene scene, TmxObject obj, GameState gameState)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var oneUp = scene.CreateEntity("oneup", center);
            oneUp.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Green);
            oneUp.AddComponent(new Mover());
            oneUp.AddComponent(new GravityBody());

            var body = oneUp.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.PickupBody;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var pickup = oneUp.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.PickupTrigger;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            oneUp.AddComponent(new OneUp(gameState));
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (_collected || other.Entity.GetComponent<PlayerController>() == null)
                return;

            _collected = true;
            _gameState.Lives++;
            Audio.PlaySfx(Assets.Sfx.PickupOneUp);
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
