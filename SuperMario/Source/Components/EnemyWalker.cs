using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class EnemyWalker : Component, IUpdatable
    {
        readonly Mover _mover;
        readonly float _speed;
        int _direction;

        public EnemyWalker(Mover mover, float speed, int direction = -1)
        {
            _mover = mover;
            _speed = speed;
            _direction = direction < 0 ? -1 : 1;
        }

        public void Update()
        {
            var motion = new Vector2(_direction * _speed * Time.DeltaTime, 0);
            _mover.CalculateMovement(ref motion, out var result);
            _mover.ApplyMovement(motion);

            if (result.Collider != null && result.Normal.X != 0)
                _direction *= -1;
        }
    }
}
