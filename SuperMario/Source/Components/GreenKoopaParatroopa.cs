using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class GreenKoopaParatroopa : Component, IUpdatable, IFireballHittable
    {
        readonly EntityFactory _factory;
        readonly Mover _mover;
        readonly float _width;
        readonly float _height;
        readonly float _flySpeed;
        int _direction = -1;

        public GreenKoopaParatroopa(EntityFactory factory, Mover mover, float width, float height, float flySpeed)
        {
            _factory = factory;
            _mover = mover;
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
            _factory.CreateGreenKoopaTroopa(Entity.Scene, Entity.Position, _width, _height);
            Entity.Destroy();
        }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
