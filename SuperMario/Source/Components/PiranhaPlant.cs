using Microsoft.Xna.Framework;
using Nez;
using Nez.Tweens;

namespace SuperMario
{
    public class PiranhaPlant : Component, IFireballHittable
    {
        const float EmergeDuration = 1.0f;
        const float PauseDuration = 1.5f;

        readonly Vector2 _exposedPosition;

        public PiranhaPlant(Vector2 exposedPosition)
        {
            _exposedPosition = exposedPosition;
        }

        public override void OnAddedToEntity()
        {
            Entity.Transform.TweenPositionTo(_exposedPosition, EmergeDuration)
                .SetEaseType(EaseType.Linear)
                .SetLoops(LoopType.PingPong, -1, PauseDuration)
                .Start();
        }

        public override void OnRemovedFromEntity()
        {
            TweenManager.StopAllTweensWithTarget(Entity.Transform);
        }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
