using Nez;

namespace SuperMario
{
    public class Goomba : Component, IFireballHittable, IStompable
    {
        public void OnStomped(PlayerController player)
        {
            Entity.Destroy();
        }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
