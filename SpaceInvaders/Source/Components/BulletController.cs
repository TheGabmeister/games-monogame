using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Systems;

namespace SpaceInvaders
{
    public class BulletController : Component, IUpdatable, ITriggerListener
    {
        readonly bool _isPlayerBullet;
        readonly float _speed;
        ProjectileMover _mover;

        public BulletController(bool isPlayerBullet)
        {
            _isPlayerBullet = isPlayerBullet;
            _speed = isPlayerBullet ? Constants.PlayerBulletSpeed : Constants.EnemyBulletSpeed;
        }

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<ProjectileMover>();
        }

        public void Update()
        {
            float direction = _isPlayerBullet ? -1 : 1;
            var motion = new Vector2(0, direction * _speed * Time.DeltaTime);
            _mover.Move(motion);

            var y = Entity.Transform.Position.Y;
            if (y < -20 || y > Constants.ScreenHeight + 20)
                Entity.Destroy();
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }

        public static Entity CreateBullet(Scene scene, Vector2 position, bool isPlayerBullet)
        {
            var bullet = scene.CreateEntity("bullet", position);

            var texturePath = isPlayerBullet
                ? Assets.Sprites.Effects.BulletPlayer
                : Assets.Sprites.Effects.BulletEnemy;
            bullet.AddComponent(new SpriteRenderer(scene.Content.LoadTexture(texturePath, true)));
            bullet.Transform.SetScale(0.4f);

            var collider = bullet.AddComponent(new BoxCollider(6, 16));
            if (isPlayerBullet)
            {
                Flags.SetFlagExclusive(ref collider.PhysicsLayer, PhysicsLayers.PlayerBullet);
                Flags.SetFlag(ref collider.CollidesWithLayers, PhysicsLayers.Invader);
                Flags.SetFlag(ref collider.CollidesWithLayers, PhysicsLayers.Shield);
            }
            else
            {
                Flags.SetFlagExclusive(ref collider.PhysicsLayer, PhysicsLayers.EnemyBullet);
                Flags.SetFlag(ref collider.CollidesWithLayers, PhysicsLayers.Player);
                Flags.SetFlag(ref collider.CollidesWithLayers, PhysicsLayers.Shield);
                bullet.Tag = Tags.EnemyBullet;
            }

            bullet.AddComponent<ProjectileMover>();
            bullet.AddComponent(new BulletController(isPlayerBullet));

            return bullet;
        }
    }
}
