using Nez;

namespace SuperMario
{
    public class Mushroom : Component, ITriggerListener
    {

        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player != null)
            {
                Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PickupMushroom);
                player.GrowPlayer();
                Entity.Destroy();
            }
                
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
