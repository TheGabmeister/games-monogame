using Nez;

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

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (_collected || other.Entity.GetComponent<PlayerController>() == null)
                return;

            _collected = true;
            _gameState.AddScore(CoinValue);
            ScorePopup.Spawn(Entity.Scene, Entity.Position, CoinValue);
            Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PickupCoin);
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
