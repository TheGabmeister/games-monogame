using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class KoopaParatroopa : Component, IUpdatable, IFireballHittable
    {
        readonly EntityFactory _factory;
        readonly Mover _mover;
        readonly KoopaColor _color;
        readonly float _width;
        readonly float _height;
        readonly float _flySpeed;
        int _direction = -1;

        public KoopaParatroopa(
            EntityFactory factory,
            Mover mover,
            KoopaColor color,
            float width,
            float height,
            float flySpeed)
        {
            _factory = factory;
            _mover = mover;
            _color = color;
            _width = width;
            _height = height;
            _flySpeed = flySpeed;
        }

        public void Update()
        {
            var motion = new Vector2(_direction * _flySpeed * Time.DeltaTime, 0);
            _mover.CalculateMovement(ref motion, out var result);
            _mover.ApplyMovement(motion);

            if (result.Collider != null && result.Normal.X != 0)
                _direction *= -1;
        }

        public void OnStomped()
        {
            _factory.CreateKoopaTroopa(Entity.Scene, Entity.Position, _width, _height, _color);
            Entity.Destroy();
        }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
