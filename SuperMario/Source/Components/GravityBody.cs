using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class GravityBody : Component, IUpdatable
    {
        public Vector2 Velocity;
        public float Gravity = 1200f;
        public float MaxFallSpeed = 600f;
        public bool IsGrounded { get; private set; }
        public CollisionResult LastCollision { get; private set; }

        Mover _mover;

        public GravityBody() => UpdateOrder = 5;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();
        }

        public void Update()
        {
            Velocity.Y += Gravity * Time.DeltaTime;
            if (Velocity.Y > MaxFallSpeed)
                Velocity.Y = MaxFallSpeed;

            var motion = Velocity * Time.DeltaTime;
            _mover.CalculateMovement(ref motion, out var result);
            _mover.ApplyMovement(motion);

            LastCollision = result;
            IsGrounded = result.Collider != null && result.Normal.Y < 0;

            if (result.Collider != null)
            {
                if (result.Normal.Y < 0 && Velocity.Y > 0) Velocity.Y = 0;
                if (result.Normal.Y > 0 && Velocity.Y < 0) Velocity.Y = 0;
                if (result.Normal.X < 0 && Velocity.X > 0) Velocity.X = 0;
                if (result.Normal.X > 0 && Velocity.X < 0) Velocity.X = 0;
            }
        }
    }
}
