using Nez;

namespace SuperMario
{
    public class DamagePlayerTrigger : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player == null)
                return;

            player.TakeDamage();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
