using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class Hammer : Component, IUpdatable, ITriggerListener
    {
        Mover _mover;
        Vector2 _velocity;

        public Hammer(int facing)
        {
            _velocity = new Vector2(facing * Constants.HammerHorizontalSpeed, -Constants.HammerInitialUpSpeed);
        }

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();
        }

        public void Update()
        {
            _velocity.Y += Constants.HammerGravity * Time.DeltaTime;
            var motion = _velocity * Time.DeltaTime;
            _mover.ApplyMovement(motion);
            Entity.Transform.Rotation += Constants.HammerSpinSpeed * Time.DeltaTime;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player == null)
                return;

            player.TakeDamage();
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
