using System.Collections.Generic;
using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Consumes the player's bomb intent: spends one bomb to clear every enemy bullet on
    // screen and blow up every enemy, with a moment of i-frames so triggering it is
    // always a safe panic button (see PLAN.md §5). Enemies are knocked to 0 HP and reaped
    // by the DamageSystem like any other kill, so they still score and drop pickups.
    public class BombSystem : EntityUpdateSystem
    {
        // Set by Game1 after the World is built (factory-wiring note in AGENTS.md).
        public AudioManager Audio;
        public GameState State;

        // Brief invulnerability covering the screen-clear so a bomb is never a death trap.
        private const float BombInvuln = 1.5f;

        private ComponentMapper<Player> _playerMapper;
        private ComponentMapper<CircleCollider> _colliderMapper;
        private ComponentMapper<Health> _healthMapper;

        private readonly List<int> _entities = new();

        public BombSystem() : base(Aspect.All(typeof(Transform), typeof(CircleCollider))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _playerMapper = mapperService.GetMapper<Player>();
            _colliderMapper = mapperService.GetMapper<CircleCollider>();
            _healthMapper = mapperService.GetMapper<Health>();
        }

        public override void Update(GameTime gameTime)
        {
            if (State != null && State.Phase != GamePhase.Playing)
                return;

            _entities.Clear();
            _entities.AddRange(ActiveEntities);

            // Find a player who pressed bomb this frame and still has stock to spend.
            bool fire = false;
            foreach (var id in _entities)
            {
                if (!_playerMapper.Has(id)) continue;
                var player = _playerMapper.Get(id);
                if (player.BombPressed && player.Bombs > 0)
                {
                    player.Bombs--;
                    if (player.InvulnTimer < BombInvuln)
                        player.InvulnTimer = BombInvuln;
                    fire = true;
                }
            }

            if (!fire)
                return;

            Audio?.Play(Assets.Sfx.Bomb);

            foreach (var id in _entities)
            {
                var layer = _colliderMapper.Get(id).Layer;
                if (layer == CollisionLayer.EnemyBullet)
                    DestroyEntity(id);
                else if (layer == CollisionLayer.Enemy && _healthMapper.Has(id))
                    _healthMapper.Get(id).Current = 0; // DamageSystem turns this into a kill
            }
        }
    }
}
