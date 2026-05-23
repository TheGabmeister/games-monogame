using Nez;

namespace SuperMario
{
    public class Goomba : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player != null)
            {
                Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PlayerHit);
                player.KillPlayer();
            }
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
