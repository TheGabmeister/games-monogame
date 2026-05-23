using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class Coin : Component, ITriggerListener
    {
        const int CoinValue = 200;

        readonly GameState _gameState;
        bool _collected;

        public Coin(GameState gameState)
        {
            _gameState = gameState;
        }

        public static void Spawn(Scene scene, TmxObject obj, GameState gameState)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var coin = scene.CreateEntity("coin", center);
            coin.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Gold);

            var pickup = coin.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.PickupTrigger;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            coin.AddComponent(new Coin(gameState));
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (_collected || other.Entity.GetComponent<PlayerController>() == null)
                return;

            _collected = true;
            _gameState.AddScore(CoinValue);
            ScorePopup.Spawn(Entity.Scene, Entity.Position, CoinValue);
            Audio.PlaySfx(Assets.Sfx.PickupCoin);
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
