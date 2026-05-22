using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace SuperMario.Components
{
    public class PlayerController : Component, IUpdatable
    {
        const float Gravity = 1200f;
        const float JumpForce = -680f;
        const float MaxFallSpeed = 600f;

        VirtualIntegerAxis _moveAxis;
        VirtualButton _jumpButton;
        Mover _mover;
        Vector2 _velocity;
        bool _grounded;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();

            _moveAxis = new VirtualIntegerAxis();
            _moveAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right);
            _moveAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D);

            _jumpButton = new VirtualButton();
            _jumpButton.AddKeyboardKey(Keys.Space);
            _jumpButton.AddKeyboardKey(Keys.Up);
            _jumpButton.AddKeyboardKey(Keys.W);
        }

        public override void OnRemovedFromEntity()
        {
            _moveAxis.Deregister();
            _jumpButton.Deregister();
        }

        public void Update()
        {
            _velocity.X = _moveAxis.Value * Constants.PlayerSpeed;

            if (_grounded && _jumpButton.IsPressed)
                _velocity.Y = JumpForce;

            _velocity.Y += Gravity * Time.DeltaTime;
            if (_velocity.Y > MaxFallSpeed)
                _velocity.Y = MaxFallSpeed;

            var movement = _velocity * Time.DeltaTime;
            var result = new CollisionResult();
            _mover.CalculateMovement(ref movement, out result);
            _mover.ApplyMovement(movement);

            _grounded = false;
            if (result.Collider != null && result.Normal.Y < 0)
                _grounded = true;

            if (result.Collider != null)
            {
                if (result.Normal.Y < 0 && _velocity.Y > 0)
                    _velocity.Y = 0;
                if (result.Normal.Y > 0 && _velocity.Y < 0)
                    _velocity.Y = 0;
            }
        }
    }
}
