using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Destroys entities that have outlived their usefulness. For now that means
    // bullets that have travelled off the playfield; a timed-life branch can be
    // added when explosions/effects need it.
    public class LifetimeSystem : EntityProcessingSystem
    {
        // Generous margin so an entity is fully out of view before it's removed.
        private const float Margin = 64f;

        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<Lifetime> _lifetimeMapper;

        public LifetimeSystem() : base(Aspect.All(typeof(Transform), typeof(Lifetime))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform>();
            _lifetimeMapper = mapperService.GetMapper<Lifetime>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var lifetime = _lifetimeMapper.Get(entityId);

            if (lifetime.HasTimer)
            {
                lifetime.SecondsRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (lifetime.SecondsRemaining <= 0f)
                {
                    DestroyEntity(entityId);
                    return;
                }
            }

            if (!lifetime.DespawnWhenOffscreen)
                return;

            var pos = _transformMapper.Get(entityId).Position;
            if (pos.X < -Margin || pos.X > VirtualResolution.Width + Margin ||
                pos.Y < -Margin || pos.Y > VirtualResolution.Height + Margin)
            {
                DestroyEntity(entityId);
            }
        }
    }
}
