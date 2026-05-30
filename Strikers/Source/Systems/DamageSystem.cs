using System;
using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Reaps anything whose Health has hit zero (see PLAN.md §4). An enemy blows up, awards
    // its score, and may drop a power-up. The player blows up and, if it has lives in
    // reserve, respawns with i-frames (and drops a weapon level, arcade-style); when the
    // last life is gone the run ends in GAME OVER. The damage itself is applied upstream
    // by the CollisionSystem / BombSystem.
    public class DamageSystem : EntityProcessingSystem
    {
        // Set by Game1 after the World is built (factory-wiring note in AGENTS.md).
        public EntityFactory Factory;
        public SfxManager Sfx;
        public GameState State;

        // Invulnerability granted on respawn so the player isn't instantly re-killed.
        private const float RespawnInvuln = 2.5f;

        private readonly Random _rng = new();

        private ComponentMapper<Health> _healthMapper;
        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<Player> _playerMapper;
        private ComponentMapper<Enemy> _enemyMapper;

        public DamageSystem() : base(Aspect.All(typeof(Health), typeof(Transform))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _healthMapper = mapperService.GetMapper<Health>();
            _transformMapper = mapperService.GetMapper<Transform>();
            _playerMapper = mapperService.GetMapper<Player>();
            _enemyMapper = mapperService.GetMapper<Enemy>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var health = _healthMapper.Get(entityId);
            if (health.Current > 0)
                return;

            var transform = _transformMapper.Get(entityId);
            Factory.CreateExplosion(transform.Position);

            if (_playerMapper.Has(entityId))
                KillPlayer(entityId, health, transform);
            else
                KillEnemy(entityId, transform.Position);
        }

        private void KillPlayer(int entityId, Health health, Transform transform)
        {
            Sfx?.Play(Assets.Sfx.PlayerExplode);
            var player = _playerMapper.Get(entityId);

            if (player.Lives > 0)
            {
                // Spend a life: respawn at the start, restore health, grant i-frames, and
                // knock the weapon down a level (death costs power, classic arcade rule).
                player.Lives--;
                transform.Position = EntityFactory.PlayerSpawn;
                health.Current = health.Max;
                player.InvulnTimer = RespawnInvuln;
                if (player.WeaponLevel > 1)
                    player.WeaponLevel--;
            }
            else
            {
                // Last ship down: end the run and remove the player.
                if (State != null)
                    State.Phase = GamePhase.GameOver;
                DestroyEntity(entityId);
            }
        }

        private void KillEnemy(int entityId, Vector2 position)
        {
            Sfx?.Play(Assets.Sfx.EnemyExplode);

            if (_enemyMapper.Has(entityId))
            {
                var enemy = _enemyMapper.Get(entityId);
                if (State != null)
                    State.Score += enemy.ScoreValue;
                MaybeDropPowerUp(enemy.Type, position);
            }

            DestroyEntity(entityId);
        }

        // Drop rates by archetype: gunships always cough up a weapon level, fighters give
        // a bomb or some points, popcorn occasionally drops a medal (see PLAN.md §5).
        private void MaybeDropPowerUp(EnemyType type, Vector2 position)
        {
            switch (type)
            {
                case EnemyType.Gunship:
                    Factory.CreatePowerUp(PowerUpKind.Weapon, position);
                    break;
                case EnemyType.Fighter:
                    Factory.CreatePowerUp(
                        _rng.NextDouble() < 0.5 ? PowerUpKind.Bomb : PowerUpKind.Score, position);
                    break;
                case EnemyType.Popcorn:
                    if (_rng.NextDouble() < 0.15)
                        Factory.CreatePowerUp(PowerUpKind.Score, position);
                    break;
            }
        }
    }
}
