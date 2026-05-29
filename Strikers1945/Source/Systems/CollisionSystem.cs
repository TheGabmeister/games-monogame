using System.Collections.Generic;
using Extended.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Extended.Systems
{
    // Brute-force circle collision filtered by layer/mask (see PLAN.md §4). Every pair
    // of colliders whose layers mask each other is distance-tested; on a hit the
    // matching rule applies the effect. Damage lands on Health here; turning 0-HP
    // entities into explosions/deaths is the DamageSystem's job. O(n^2) is fine at v1
    // scale (one player, few enemies, modest bullets) — revisit only if profiling says so.
    public class CollisionSystem : EntityUpdateSystem
    {
        // Shared one-shot SFX service; set by Game1 after the World is built.
        public AudioManager Audio;

        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<CircleCollider> _colliderMapper;
        private ComponentMapper<Bullet> _bulletMapper;
        private ComponentMapper<Health> _healthMapper;

        private readonly List<int> _entities = new();
        private readonly HashSet<int> _consumed = new();

        public CollisionSystem() : base(Aspect.All(typeof(Transform), typeof(CircleCollider))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform>();
            _colliderMapper = mapperService.GetMapper<CircleCollider>();
            _bulletMapper = mapperService.GetMapper<Bullet>();
            _healthMapper = mapperService.GetMapper<Health>();
        }

        public override void Update(GameTime gameTime)
        {
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
                    Audio?.Play("audio/sfx/sfx_enemy_hit");
                }
                return;
            }

            // Player rams an enemy: lethal to the player (DamageSystem handles the death).
            if (TryPair(c1, c2, e1, e2, CollisionLayer.Player, CollisionLayer.Enemy,
                        out int playerId, out int _))
            {
                if (_healthMapper.Has(playerId))
                    _healthMapper.Get(playerId).Current = 0;
            }
        }

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
