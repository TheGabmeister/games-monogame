using Nez;

namespace SpaceInvaders
{
    public class ShieldChunk : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
