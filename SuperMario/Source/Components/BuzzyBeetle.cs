using Nez;

namespace SuperMario
{
    public class BuzzyBeetle : Component, IFireballHittable, IStompable
    {
        public void OnStomped(PlayerController player)
        {
            Entity.Destroy();
        }

        public FireballReaction OnHitByFireball()
        {
            return FireballReaction.Blocked;
        }
    }
}
