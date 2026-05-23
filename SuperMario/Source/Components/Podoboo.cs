using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class Podoboo : Component, IUpdatable, IFireballHittable
    {
        Mover _mover;
        Vector2 _homePosition;
        Vector2 _velocity;
        float _restTimer;
        bool _resting;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();
            _homePosition = Entity.Position;
            Launch();
        }

        public void Update()
        {
            if (_resting)
            {
                _restTimer -= Time.DeltaTime;
                if (_restTimer <= 0f)
                    Launch();
                return;
            }

            _velocity.Y += Constants.PodobooGravity * Time.DeltaTime;
            var motion = _velocity * Time.DeltaTime;
            _mover.ApplyMovement(motion);

            if (_velocity.Y > 0f && Entity.Position.Y >= _homePosition.Y)
            {
                Entity.Position = _homePosition;
                _velocity = Vector2.Zero;
                _resting = true;
                _restTimer = Constants.PodobooRestDuration;
            }
        }

        void Launch()
        {
            _resting = false;
            _velocity = new Vector2(0f, -Constants.PodobooJumpSpeed);
        }

        public FireballReaction OnHitByFireball()
        {
            return FireballReaction.Blocked;
        }
    }
}
