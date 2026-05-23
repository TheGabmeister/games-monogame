using Nez;

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

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (_collected || other.Entity.GetComponent<PlayerController>() == null)
                return;

            _collected = true;
            _gameState.Lives++;
            Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PickupOneUp);
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
