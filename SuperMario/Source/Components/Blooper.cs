using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class Blooper : Component, IUpdatable, IFireballHittable
    {
        readonly Mover _mover;
        float _bobTimer;
        PlayerController _player;

        public Blooper(Mover mover)
        {
            _mover = mover;
        }

        public void Update()
        {
            if (_player == null || _player.Entity == null || _player.Entity.IsDestroyed)
                _player = Entity.Scene?.FindComponentOfType<PlayerController>();

            var motion = Vector2.Zero;
            if (_player != null)
            {
                var toPlayer = _player.Entity.Position - Entity.Position;
                if (toPlayer.LengthSquared() > 1f)
                    motion = Vector2.Normalize(toPlayer) * Constants.BlooperSpeed;
            }

            _bobTimer += Time.DeltaTime * Constants.BlooperBobSpeed;
            motion.Y += Mathf.Sin(_bobTimer) * Constants.BlooperBobStrength;

            motion *= Time.DeltaTime;
            _mover.CalculateMovement(ref motion, out _);
            _mover.ApplyMovement(motion);
        }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
