using Extended.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Extended.Systems
{
    internal class PlayerSystem : EntityProcessingSystem
    {
        private ComponentMapper<Player> _playerMapper;

        public PlayerSystem() : base(Aspect.All(typeof(Player))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _playerMapper = mapperService.GetMapper<Player>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            Player player = _playerMapper.Get(entityId);

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                player.Position.X -= 5;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                player.Position.X += 5;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                player.Position.Y -= 5;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                player.Position.Y += 5;
        }
    }
}
