using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

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

        public static void SpawnLeftRight(Scene scene, TmxObject obj)
        {
            Spawn(scene, obj, MovingPlatformAxis.Horizontal);
        }

        public static void SpawnUpDown(Scene scene, TmxObject obj)
        {
            Spawn(scene, obj, MovingPlatformAxis.Vertical);
        }

        static void Spawn(Scene scene, TmxObject obj, MovingPlatformAxis axis)
        {
            var center = EntityFactory.GetCenter(obj);
            var platform = scene.CreateEntity(obj.Name, center);
            platform.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.SteelBlue);

            var collider = platform.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.Item);

            platform.AddComponent(new MovingPlatform(
                axis,
                Constants.MovingPlatformDistance,
                Constants.MovingPlatformSpeed));
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
