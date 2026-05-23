using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class BulletBillCannon : Component, IUpdatable
    {
        PlayerController _player;
        float _fireTimer;

        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var cannon = scene.CreateEntity("bulletbillcannon", center);
            cannon.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.DimGray);

            var body = cannon.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Environment;
            body.CollidesWithLayers = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.PickupBody);

            cannon.AddComponent(new BulletBillCannon());
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
            BulletBill.Spawn(Entity.Scene, Entity.Position, facing);
        }
    }
}
