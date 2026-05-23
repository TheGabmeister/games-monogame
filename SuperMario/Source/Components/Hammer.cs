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

        public static void Spawn(Scene scene, Vector2 position, int facing)
        {
            const int size = 16;

            var hammer = scene.CreateEntity("hammer", position);
            hammer.AddComponent(new PrototypeSpriteRenderer(size, size)).SetColor(Color.Gray);
            hammer.AddComponent(new Mover());

            var hit = hammer.AddComponent(new BoxCollider(-size / 2f, -size / 2f, size, size));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.EnemyProjectile;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            hammer.AddComponent(new Hammer(facing));
            hammer.AddComponent(new Lifetime(Constants.HammerLifetime));
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
