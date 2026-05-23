using Nez;

namespace SuperMario
{
    public class BulletBillCannon : Component, IUpdatable
    {
        readonly EntityFactory _factory;
        PlayerController _player;
        float _fireTimer;

        public BulletBillCannon(EntityFactory factory)
        {
            _factory = factory;
        }

        public override void OnAddedToEntity()
        {
            _fireTimer = Constants.BulletBillCannonFireInterval * 0.5f;
        }

        public void Update()
        {
            if (_player == null || _player.Entity == null || _player.Entity.IsDestroyed)
                _player = Entity.Scene?.FindComponentOfType<PlayerController>();

            _fireTimer += Time.DeltaTime;
            if (_fireTimer < Constants.BulletBillCannonFireInterval)
                return;

            _fireTimer = 0f;
            var facing = _player != null && _player.Entity.Position.X < Entity.Position.X ? -1 : 1;
            _factory.CreateBulletBill(Entity.Scene, Entity.Position, facing);
        }
    }
}
