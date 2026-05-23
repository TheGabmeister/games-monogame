using Nez;

namespace SuperMario
{
    public class Goomba : Component, ITriggerListener, IFireballHittable
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player != null)
            {
                Audio.PlaySfx(Assets.Sfx.PlayerHit);
                player.KillPlayer();
            }
        }

        public void OnTriggerExit(Collider other, Collider local) { }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
