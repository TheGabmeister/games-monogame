using Nez;

namespace SuperMario
{
    public class KoopaTroopa : Component, IFireballHittable, IStompable
    {
        public KoopaTroopa(KoopaColor color)
        {
            Color = color;
        }

        public KoopaColor Color { get; }

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
