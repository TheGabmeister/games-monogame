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

            var stompable = Entity.GetComponent<IStompable>();
            if (stompable != null && player.TryStomp(local, stompable))
                return;

            player.TakeDamage();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
