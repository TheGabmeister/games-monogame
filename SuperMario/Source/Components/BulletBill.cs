using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class BulletBill : Component, IUpdatable, IFireballHittable
    {
        Mover _mover;
        Vector2 _velocity;

        public BulletBill(int facing)
        {
            _velocity = new Vector2(facing * Constants.BulletBillSpeed, 0f);
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
    }
}
