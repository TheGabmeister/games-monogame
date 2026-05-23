using Nez;

namespace SuperMario
{
    public class GoalTrigger : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player == null)
                return;

            if (Entity.Scene is GameplayScene gameplayScene)
                gameplayScene.CompleteLevel();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
