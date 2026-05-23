using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class MovingPlatform : Component, IUpdatable
    {
        readonly MovingPlatformAxis _axis;
        readonly float _distance;
        readonly float _speed;
        Vector2 _start;
        Vector2 _end;
        int _direction = 1;

        public Vector2 DeltaPosition { get; private set; }

        public MovingPlatform(MovingPlatformAxis axis, float distance, float speed)
        {
            _axis = axis;
            _distance = distance;
            _speed = speed;
        }

        public override void OnAddedToEntity()
        {
            _start = Entity.Position;
            _end = _axis == MovingPlatformAxis.Horizontal
                ? _start + new Vector2(_distance, 0f)
                : _start + new Vector2(0f, _distance);
        }

        public void Update()
        {
            var previous = Entity.Position;
            var target = _direction > 0 ? _end : _start;
            var toTarget = target - Entity.Position;
            var maxDistance = _speed * Time.DeltaTime;

            if (toTarget.LengthSquared() <= maxDistance * maxDistance)
            {
                Entity.Position = target;
                _direction *= -1;
            }
            else
            {
                toTarget.Normalize();
                Entity.Position += toTarget * maxDistance;
            }

            DeltaPosition = Entity.Position - previous;
        }
    }
}
