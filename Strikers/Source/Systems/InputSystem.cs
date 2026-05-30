using Strikers.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Reads keyboard + gamepad and normalizes both into the player's MoveDirection
    // intent, so no other system has to know which device is in use.
    public class InputSystem : EntityProcessingSystem
    {
        private ComponentMapper<Player> _playerMapper;

        // Previous-frame bomb-button state, so the bomb fires once per press (edge), not
        // every frame it's held.
        private bool _bombHeld;

        public InputSystem() : base(Aspect.All(typeof(Player))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _playerMapper = mapperService.GetMapper<Player>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var player = _playerMapper.Get(entityId);
            var dir = Vector2.Zero;

            var kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Left)  || kb.IsKeyDown(Keys.A)) dir.X -= 1;
            if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D)) dir.X += 1;
            if (kb.IsKeyDown(Keys.Up)    || kb.IsKeyDown(Keys.W)) dir.Y -= 1;
            if (kb.IsKeyDown(Keys.Down)  || kb.IsKeyDown(Keys.S)) dir.Y += 1;

            var firing = kb.IsKeyDown(Keys.Space) || kb.IsKeyDown(Keys.Z);
            var bombHeld = kb.IsKeyDown(Keys.X) || kb.IsKeyDown(Keys.LeftShift);

            var pad = GamePad.GetState(PlayerIndex.One);
            if (pad.IsConnected)
            {
                if (pad.DPad.Left  == ButtonState.Pressed) dir.X -= 1;
                if (pad.DPad.Right == ButtonState.Pressed) dir.X += 1;
                if (pad.DPad.Up    == ButtonState.Pressed) dir.Y -= 1;
                if (pad.DPad.Down  == ButtonState.Pressed) dir.Y += 1;

                // Left stick: Y points up on the stick but down on screen.
                dir.X += pad.ThumbSticks.Left.X;
                dir.Y -= pad.ThumbSticks.Left.Y;

                if (pad.Buttons.A == ButtonState.Pressed) firing = true;
                if (pad.Buttons.B == ButtonState.Pressed || pad.Buttons.X == ButtonState.Pressed)
                    bombHeld = true;
            }

            // Cap to unit length so diagonals/stick don't move faster than cardinal.
            if (dir.LengthSquared() > 1f)
                dir.Normalize();

            player.MoveDirection = dir;
            player.Firing = firing;
            // Edge-trigger: true only on the frame the bomb button goes down.
            player.BombPressed = bombHeld && !_bombHeld;
            _bombHeld = bombHeld;
        }
    }
}
