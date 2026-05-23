using Nez;

namespace SuperMario
{
    public class RedKoopaTroopa : Component, IFireballHittable
    {
        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
