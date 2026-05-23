using Nez;

namespace SuperMario
{
    public class GreenKoopaTroopa : Component, IFireballHittable
    {
        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
