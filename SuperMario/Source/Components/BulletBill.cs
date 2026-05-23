using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class BulletBill : Component, IUpdatable, IFireballHittable, IStompable, IStarHittable
    {
        Mover _mover;
        Vector2 _velocity;

        public BulletBill(int facing)
        {
            _velocity = new Vector2(facing * Constants.BulletBillSpeed, 0f);
        }

        public static void Spawn(Scene scene, Vector2 position, int facing)
        {
            const int size = 24;

            var bill = scene.CreateEntity("bulletbill", position);
            bill.AddComponent(new PrototypeSpriteRenderer(size, size)).SetColor(Color.Black);
            bill.AddComponent(new Mover());

            var hit = bill.AddComponent(new BoxCollider(-size / 2f, -size / 2f, size, size));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            bill.AddComponent(new DamagePlayerTrigger());
            bill.AddComponent(new BulletBill(facing));
            bill.AddComponent(new Lifetime(Constants.BulletBillLifetime));
        }

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();
        }

        public void Update()
        {
            var motion = _velocity * Time.DeltaTime;
            _mover.ApplyMovement(motion);
        }

        public FireballReaction OnHitByFireball()
        {
            return FireballReaction.Blocked;
        }

        public void OnStomped(PlayerController player)
        {
            Entity.Destroy();
        }

        public void OnHitByStar(PlayerController player)
        {
            Entity.Destroy();
        }
    }
}
