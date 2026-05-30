using System.Collections.Generic;
using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Brute-force circle collision filtered by layer/mask (see PLAN.md §4). Every pair
    // of colliders whose layers mask each other is distance-tested; on a hit the
    // matching rule applies the effect. Damage lands on Health here; turning 0-HP
    // entities into explosions/deaths is the DamageSystem's job. O(n^2) is fine at v1
    // scale (one player, few enemies, modest bullets) — revisit only if profiling says so.
    public class CollisionSystem : EntityUpdateSystem
    {
        // Shared one-shot SFX service; set by Game1 after the World is built.
        public SfxManager Sfx;
        public GameState State;

        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<CircleCollider> _colliderMapper;
        private ComponentMapper<Bullet> _bulletMapper;
        private ComponentMapper<Health> _healthMapper;
        private ComponentMapper<Player> _playerMapper;
        private ComponentMapper<PowerUp> _powerUpMapper;

        private readonly List<int> _entities = new();
        private readonly HashSet<int> _consumed = new();

        private const float GrazePadding = 18f;
        private const int GrazeScore = 10;

        public CollisionSystem() : base(Aspect.All(typeof(Transform), typeof(CircleCollider))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform>();
            _colliderMapper = mapperService.GetMapper<CircleCollider>();
            _bulletMapper = mapperService.GetMapper<Bullet>();
            _healthMapper = mapperService.GetMapper<Health>();
            _playerMapper = mapperService.GetMapper<Player>();
            _powerUpMapper = mapperService.GetMapper<PowerUp>();
        }

        public override void Update(GameTime gameTime)
        {
            // No collisions once the run ends — leftover bullets become harmless.
            if (State != null && State.Phase != GamePhase.Playing)
                return;

            _entities.Clear();
            _entities.AddRange(ActiveEntities);
            _consumed.Clear();

            for (int i = 0; i < _entities.Count; i++)
            {
                int a = _entities[i];
                if (_consumed.Contains(a)) continue;
                var ca = _colliderMapper.Get(a);

                for (int j = i + 1; j < _entities.Count; j++)
                {
                    if (_consumed.Contains(a)) break;

                    int b = _entities[j];
                    if (_consumed.Contains(b)) continue;
                    var cb = _colliderMapper.Get(b);

                    // Either side must want to collide with the other.
                    if ((ca.Layer & cb.Mask) == 0 && (cb.Layer & ca.Mask) == 0)
                        continue;

                    if (!Overlaps(a, ca, b, cb))
                        continue;

                    Resolve(a, ca, b, cb);
                }
            }

            AwardGrazes();
        }

        private bool Overlaps(int a, CircleCollider ca, int b, CircleCollider cb)
        {
            var pa = _transformMapper.Get(a).Position;
            var pb = _transformMapper.Get(b).Position;
            float r = ca.Radius + cb.Radius;
            return Vector2.DistanceSquared(pa, pb) <= r * r;
        }

        private void Resolve(int e1, CircleCollider c1, int e2, CircleCollider c2)
        {
            // Player bullet strikes an enemy: chip its health, spend the bullet.
            if (TryPair(c1, c2, e1, e2, CollisionLayer.PlayerBullet, CollisionLayer.Enemy,
                        out int bulletId, out int enemyId))
            {
                if (_healthMapper.Has(enemyId) && _bulletMapper.Has(bulletId))
                {
                    _healthMapper.Get(enemyId).Current -= _bulletMapper.Get(bulletId).Damage;
                    DestroyEntity(bulletId);
                    _consumed.Add(bulletId);
                    Sfx?.Play(Assets.Sfx.EnemyHit);
                }
                return;
            }

            // Enemy bullet strikes the player: lethal unless invulnerable; spend the bullet.
            // While invulnerable the bullet passes through untouched.
            if (TryPair(c1, c2, e1, e2, CollisionLayer.EnemyBullet, CollisionLayer.Player,
                        out int enemyBulletId, out int hitPlayerId))
            {
                if (PlayerVulnerable(hitPlayerId))
                {
                    _healthMapper.Get(hitPlayerId).Current = 0;
                    DestroyEntity(enemyBulletId);
                    _consumed.Add(enemyBulletId);
                }
                return;
            }

            // Player touches a power-up: apply it and consume the pickup.
            if (TryPair(c1, c2, e1, e2, CollisionLayer.Player, CollisionLayer.PowerUp,
                        out int collectorId, out int pickupId))
            {
                if (_powerUpMapper.Has(pickupId))
                {
                    ApplyPickup(collectorId, _powerUpMapper.Get(pickupId).Kind);
                    DestroyEntity(pickupId);
                    _consumed.Add(pickupId);
                }
                return;
            }

            // Player rams an enemy: lethal to the player (DamageSystem handles the death).
            if (TryPair(c1, c2, e1, e2, CollisionLayer.Player, CollisionLayer.Enemy,
                        out int playerId, out int _))
            {
                if (PlayerVulnerable(playerId))
                    _healthMapper.Get(playerId).Current = 0;
            }
        }

        // Grants the pickup's effect to the collecting player (see PLAN.md §3).
        private void ApplyPickup(int playerId, PowerUpKind kind)
        {
            var player = _playerMapper.Get(playerId);
            switch (kind)
            {
                case PowerUpKind.Weapon:
                    if (player.WeaponLevel < EntityFactory.MaxWeaponLevel)
                        player.WeaponLevel++;
                    Sfx?.Play(Assets.Sfx.PowerUpWeapon);
                    break;
                case PowerUpKind.Bomb:
                    if (player.Bombs < EntityFactory.MaxBombs)
                        player.Bombs++;
                    Sfx?.Play(Assets.Sfx.PowerUp);
                    break;
                default: // Score
                    if (State != null) State.Score += 500;
                    Sfx?.Play(Assets.Sfx.PowerUp);
                    break;
            }
        }

        private void AwardGrazes()
        {
            foreach (var playerId in _entities)
            {
                if (_consumed.Contains(playerId) || !_playerMapper.Has(playerId))
                    continue;
                if (!_colliderMapper.Has(playerId) || !_transformMapper.Has(playerId))
                    continue;

                var playerCollider = _colliderMapper.Get(playerId);
                if (playerCollider.Layer != CollisionLayer.Player)
                    continue;

                var playerPos = _transformMapper.Get(playerId).Position;

                foreach (var bulletId in _entities)
                {
                    if (_consumed.Contains(bulletId) || !_bulletMapper.Has(bulletId))
                        continue;
                    if (!_colliderMapper.Has(bulletId) || !_transformMapper.Has(bulletId))
                        continue;

                    var bulletCollider = _colliderMapper.Get(bulletId);
                    if (bulletCollider.Layer != CollisionLayer.EnemyBullet)
                        continue;

                    var bullet = _bulletMapper.Get(bulletId);
                    if (bullet.Grazed)
                        continue;

                    float hitRadius = playerCollider.Radius + bulletCollider.Radius;
                    float grazeRadius = hitRadius + GrazePadding;
                    float distanceSquared = Vector2.DistanceSquared(
                        playerPos, _transformMapper.Get(bulletId).Position);

                    if (distanceSquared <= hitRadius * hitRadius ||
                        distanceSquared > grazeRadius * grazeRadius)
                    {
                        continue;
                    }

                    bullet.Grazed = true;
                    if (State != null)
                        State.Score += GrazeScore;
                    Sfx?.Play(Assets.Sfx.Graze);
                }
            }
        }

        // The player can be killed only when it has Health and isn't in its i-frames.
        private bool PlayerVulnerable(int playerId) =>
            _healthMapper.Has(playerId) &&
            !(_playerMapper.Has(playerId) && _playerMapper.Get(playerId).InvulnTimer > 0f);

        // If the two colliders are exactly {layerA, layerB}, binds ea/eb to the entity
        // carrying layerA / layerB respectively. Order-independent.
        private static bool TryPair(CircleCollider c1, CircleCollider c2, int e1, int e2,
                                    CollisionLayer layerA, CollisionLayer layerB, out int ea, out int eb)
        {
            if (c1.Layer == layerA && c2.Layer == layerB) { ea = e1; eb = e2; return true; }
            if (c2.Layer == layerA && c1.Layer == layerB) { ea = e2; eb = e1; return true; }
            ea = eb = 0;
            return false;
        }
    }
}
