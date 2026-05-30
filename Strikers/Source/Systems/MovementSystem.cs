using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Generic velocity integrator: Position += Velocity * dt. Drives bullets and
    // enemies from Phase 1 on. The player is intentionally not velocity-driven
    // (see PlayerControlSystem), so it has no Velocity and is skipped here.
    public class MovementSystem : EntityProcessingSystem
    {
        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<Velocity> _velocityMapper;

        public MovementSystem() : base(Aspect.All(typeof(Transform), typeof(Velocity))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform>();
            _velocityMapper = mapperService.GetMapper<Velocity>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _transformMapper.Get(entityId).Position += _velocityMapper.Get(entityId).Value * dt;
        }
    }
}
