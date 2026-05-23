using Nez;

namespace SuperMario
{
    public class FireFlower : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player == null)
                return;

            player.GrowPlayer();
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
