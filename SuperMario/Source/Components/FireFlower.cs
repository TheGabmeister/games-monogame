using Nez;

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
