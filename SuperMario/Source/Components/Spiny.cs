using Nez;

namespace SuperMario
{
    public class Spiny : Component, IFireballHittable
    {
        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
