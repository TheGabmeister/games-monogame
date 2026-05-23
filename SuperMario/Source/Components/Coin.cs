using Nez;

namespace SuperMario
{
    public class Coin : Component, ITriggerListener
    {
        const int CoinValue = 200;

        readonly GameState _gameState;

        public Coin(GameState gameState)
        {
            _gameState = gameState;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            _gameState.Score += CoinValue;
            Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PickupCoin);
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
