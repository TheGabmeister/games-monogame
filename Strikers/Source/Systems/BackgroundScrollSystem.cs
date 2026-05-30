using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Wraps the scrolling-background tiles. MovementSystem already slides each tile down
    // via its Velocity; once a tile has fully left the bottom of the screen this lifts it
    // back above the other one, so the two tiles loop forever (see PLAN.md §5). Tiles are
    // centered (Sprite origin is the middle), so a full-screen tile fills the view at
    // center Y = Height/2 and has fully exited once its center passes Height + Height/2.
    public class BackgroundScrollSystem : EntityProcessingSystem
    {
        private const float Wrap = VirtualResolution.Height + VirtualResolution.Height / 2f;
        private const float TwoScreens = 2f * VirtualResolution.Height;

        private ComponentMapper<Transform> _transformMapper;

        public BackgroundScrollSystem() : base(Aspect.All(typeof(Transform), typeof(Background))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = _transformMapper.Get(entityId);
            if (transform.Position.Y >= Wrap)
                transform.Position = new Vector2(transform.Position.X, transform.Position.Y - TwoScreens);
        }
    }
}
