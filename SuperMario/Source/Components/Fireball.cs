using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class Fireball : Component, IUpdatable
    {
        const float Speed = 400f;
        const float Gravity = 1400f;
        const float BounceForce = -500f;
        const float MaxFallSpeed = 700f;
        const float Lifetime = 3f;

        readonly PlayerController _owner;
        Mover _mover;
        Vector2 _velocity;
        float _age;
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
            _age += Time.DeltaTime;
            if (_age >= Lifetime)
            {
                Destroy(false);
                return;
            }

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
                Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PlayerFireHitBlock);

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
    }
}
