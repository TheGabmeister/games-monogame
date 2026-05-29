using Extended.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Extended.Systems
{
    // Moves the player from its MoveDirection intent and clamps it inside the
    // playfield. Player movement is owned here (not the generic MovementSystem)
    // because the clamp has to happen right after the move, in one place.
    public class PlayerControlSystem : EntityProcessingSystem
    {
        private ComponentMapper<Player> _playerMapper;
        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<Sprite> _spriteMapper;

        public PlayerControlSystem() : base(Aspect.All(typeof(Player), typeof(Transform))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _playerMapper = mapperService.GetMapper<Player>();
            _transformMapper = mapperService.GetMapper<Transform>();
            _spriteMapper = mapperService.GetMapper<Sprite>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var player = _playerMapper.Get(entityId);
            var transform = _transformMapper.Get(entityId);

            transform.Position += player.MoveDirection * player.Speed * dt;

            // Keep the whole sprite on-screen by clamping against its half-size.
            var half = Vector2.Zero;
            if (_spriteMapper.Has(entityId))
                half = _spriteMapper.Get(entityId).Size * 0.5f;

            transform.Position = new Vector2(
                MathHelper.Clamp(transform.Position.X, half.X, VirtualResolution.Width  - half.X),
                MathHelper.Clamp(transform.Position.Y, half.Y, VirtualResolution.Height - half.Y));
        }
    }
}
