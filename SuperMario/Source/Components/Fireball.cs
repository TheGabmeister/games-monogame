using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class Fireball : Component, IUpdatable, ITriggerListener
    {
        const float Speed = 400f;
        const float Gravity = 1400f;
        const float BounceForce = -500f;
        const float MaxFallSpeed = 700f;

        readonly PlayerController _owner;
        Mover _mover;
        Vector2 _velocity;
        bool _destroyed;

        public Fireball(PlayerController owner, int facing)
        {
            _owner = owner;
            _velocity = new Vector2(Speed * facing, 0f);
        }

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();
        }

        public void Update()
        {
            _velocity.Y += Gravity * Time.DeltaTime;
            if (_velocity.Y > MaxFallSpeed)
                _velocity.Y = MaxFallSpeed;

            var movement = _velocity * Time.DeltaTime;
            _mover.CalculateMovement(ref movement, out var result);
            _mover.ApplyMovement(movement);

            if (result.Collider != null)
            {
                if (result.Normal.Y < 0)
                    _velocity.Y = BounceForce;
                else if (result.Normal.X != 0)
                {
                    Destroy(true);
                    return;
                }
            }
        }

        void Destroy(bool playHitSfx)
        {
            if (_destroyed)
                return;
            _destroyed = true;

            if (playHitSfx)
                Audio.PlaySfx(Assets.Sfx.PlayerFireHitBlock);

            _owner?.NotifyFireballDestroyed();
            Entity.Destroy();
        }

        public override void OnRemovedFromEntity()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                _owner?.NotifyFireballDestroyed();
            }
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (_destroyed)
                return;

            var hittable = other.Entity.GetComponent<IFireballHittable>();
            if (hittable == null)
                return;

            var reaction = hittable.OnHitByFireball();
            switch (reaction)
            {
                case FireballReaction.Defeated:
                    Destroy(false);
                    Audio.PlaySfx(Assets.Sfx.PlayerFireHitEnemy);
                    break;
                case FireballReaction.Blocked:
                    Destroy(true);
                    break;
            }
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
