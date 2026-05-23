using Nez;

namespace SuperMario
{
    public class Hitbox : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player == null)
                return;

            Audio.PlaySfx(Assets.Sfx.PlayerHit);
            player.TakeHit();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
