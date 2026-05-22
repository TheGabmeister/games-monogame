using Nez;

namespace SuperMario
{
    public class KillVolume : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player != null)
                player.KillPlayer();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
