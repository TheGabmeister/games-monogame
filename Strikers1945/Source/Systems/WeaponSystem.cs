using Extended.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Extended.Systems
{
    // Ticks each weapon's cooldown and, while its owner holds fire, spawns a
    // bullet travelling straight up every FireInterval seconds. Player-only for
    // now (enemies will emit via the separate EmitterSystem in Phase 3).
    public class WeaponSystem : EntityProcessingSystem
    {
        // Set by Game1 after the World is built — the factory needs the World, which
        // doesn't exist until Build(), so it can't be a constructor argument.
        public EntityFactory Factory;
        public AudioManager Audio;

        private ComponentMapper<Weapon> _weaponMapper;
        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<Player> _playerMapper;

        public WeaponSystem()
            : base(Aspect.All(typeof(Weapon), typeof(Transform), typeof(Player)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _weaponMapper = mapperService.GetMapper<Weapon>();
            _transformMapper = mapperService.GetMapper<Transform>();
            _playerMapper = mapperService.GetMapper<Player>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var weapon = _weaponMapper.Get(entityId);

            if (weapon.Cooldown > 0f)
                weapon.Cooldown -= gameTime.GetElapsedSeconds();

            var player = _playerMapper.Get(entityId);
            if (!player.Firing || weapon.Cooldown > 0f)
                return;

            var position = _transformMapper.Get(entityId).Position;
            Factory.CreateBullet(position, new Vector2(0f, -weapon.BulletSpeed), weapon.Damage);
            weapon.Cooldown = weapon.FireInterval;
            Audio?.Play("audio/sfx/sfx_player_shot");
        }
    }
}
