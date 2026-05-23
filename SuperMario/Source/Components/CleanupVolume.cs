using Nez;

namespace SuperMario
{
    public class CleanupVolume : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player != null)
            {
                player.KillPlayer();
                return;
            }

            other.Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
