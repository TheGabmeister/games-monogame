using Nez;

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
                Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PickupMushroom);
                player.ApplyMushroom();
                Entity.Destroy();
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
