using Nez;

namespace SuperMario
{
    public class OneUp : Component, ITriggerListener
    {
        readonly GameState _gameState;

        public OneUp(GameState gameState)
        {
            _gameState = gameState;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            _gameState.Lives++;
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
