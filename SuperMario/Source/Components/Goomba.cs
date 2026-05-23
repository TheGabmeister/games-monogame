using Nez;

namespace SuperMario
{
    public class Goomba : Component, IFireballHittable
    {
        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
