using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using Nez.Tweens;

namespace SuperMario
{
    public class PiranhaPlant : Component, IFireballHittable, IStarHittable
    {
        const float EmergeDuration = 1.0f;
        const float PauseDuration = 1.5f;

        readonly Vector2 _exposedPosition;

        public PiranhaPlant(Vector2 exposedPosition)
        {
            _exposedPosition = exposedPosition;
        }

        public static void Spawn(Scene scene, TmxObject obj)
        {
            var exposedCenter = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;
            var hiddenCenter = exposedCenter + new Vector2(0, h);

            var plant = scene.CreateEntity("piranhaplant", hiddenCenter);
            plant.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Green);

            var hit = plant.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            plant.AddComponent(new DamagePlayerTrigger());
            plant.AddComponent(new PiranhaPlant(exposedCenter));
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

        public void OnHitByStar(PlayerController player)
        {
            Entity.Destroy();
        }
    }
}
