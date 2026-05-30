using System;
using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Ticks each weapon's cooldown and, while its owner holds fire, spawns the main shot
    // every FireInterval seconds. The shot's shape scales with the player's WeaponLevel —
    // a single bolt at level 1 widening into angled fans at higher levels — so the weapon
    // power-ups have a visible payoff (see PLAN.md §5).
    public class WeaponSystem : EntityProcessingSystem
    {
        // Set by Game1 after the World is built — the factory needs the World, which
        // doesn't exist until Build(), so it can't be a constructor argument.
        public EntityFactory Factory;
        public SfxManager Sfx;
        public GameState State;

        // Spread between adjacent shots in the angled patterns.
        private const float SpreadAngle = 0.16f; // radians (~9°)

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
            if (State != null && State.Phase != GamePhase.Playing)
                return;

            var weapon = _weaponMapper.Get(entityId);

            if (weapon.Cooldown > 0f)
                weapon.Cooldown -= gameTime.GetElapsedSeconds();

            var player = _playerMapper.Get(entityId);
            if (!player.Firing || weapon.Cooldown > 0f)
                return;

            var position = _transformMapper.Get(entityId).Position;
            FireVolley(position, weapon, player.WeaponLevel);
            weapon.Cooldown = weapon.FireInterval;
            Sfx?.Play(Assets.Sfx.PlayerShot);
        }

        // Bullet count and spread widen with the weapon level (capped in EntityFactory).
        private void FireVolley(Vector2 position, Weapon weapon, int level)
        {
            switch (level)
            {
                case 1:
                    Fire(position, 0f, weapon);
                    break;
                case 2:
                    Fire(position + new Vector2(-7f, 0f), 0f, weapon);
                    Fire(position + new Vector2( 7f, 0f), 0f, weapon);
                    break;
                case 3:
                    Fire(position, 0f, weapon);
                    Fire(position, -SpreadAngle, weapon);
                    Fire(position,  SpreadAngle, weapon);
                    break;
                default: // 4+
                    Fire(position + new Vector2(-8f, 0f), 0f, weapon);
                    Fire(position + new Vector2( 8f, 0f), 0f, weapon);
                    Fire(position, -SpreadAngle * 1.5f, weapon);
                    Fire(position,  SpreadAngle * 1.5f, weapon);
                    break;
            }
        }

        // Spawns one bullet travelling "up" rotated by `angle` (0 = straight up).
        private void Fire(Vector2 position, float angle, Weapon weapon)
        {
            var velocity = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * weapon.BulletSpeed;
            Factory.CreateBullet(position, velocity, weapon.Damage);
        }
    }
}
