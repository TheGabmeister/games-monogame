using Nez;

namespace SuperMario
{
    public class BuzzyBeetle : Component, IFireballHittable
    {
        public FireballReaction OnHitByFireball()
        {
            return FireballReaction.Blocked;
        }
    }
}
