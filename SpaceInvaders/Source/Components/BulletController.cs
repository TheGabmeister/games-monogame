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
            _speed = isPlayerBullet ? GameConstants.PlayerBulletSpeed : GameConstants.EnemyBulletSpeed;
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
            if (y < -20 || y > GameConstants.ScreenHeight + 20)
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
                ? ContentPaths.Sprites.Effects.BulletPlayer
                : ContentPaths.Sprites.Effects.BulletEnemy;
            bullet.AddComponent(new SpriteRenderer(scene.Content.LoadTexture(texturePath, true)));
            bullet.Transform.SetScale(0.4f);

            var collider = bullet.AddComponent(new BoxCollider(6, 16));
            if (isPlayerBullet)
            {
                collider.PhysicsLayer = 1 << PhysicsLayers.PlayerBullet;
                collider.CollidesWithLayers = PhysicsLayers.Mask(PhysicsLayers.Invader, PhysicsLayers.Shield);
                bullet.Tag = Tags.PlayerBullet;
            }
            else
            {
                collider.PhysicsLayer = 1 << PhysicsLayers.EnemyBullet;
                collider.CollidesWithLayers = PhysicsLayers.Mask(PhysicsLayers.Player, PhysicsLayers.Shield);
                bullet.Tag = Tags.EnemyBullet;
            }

            bullet.AddComponent<ProjectileMover>();
            bullet.AddComponent(new BulletController(isPlayerBullet));

            return bullet;
        }
    }
}
