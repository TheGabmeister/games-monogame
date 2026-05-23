using Nez;

namespace SuperMario
{
    public class KoopaTroopa : Component, IFireballHittable
    {
        public KoopaTroopa(KoopaColor color)
        {
            Color = color;
        }

        public KoopaColor Color { get; }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
