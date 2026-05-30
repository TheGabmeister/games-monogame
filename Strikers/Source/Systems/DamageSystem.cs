using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Reaps anything whose Health has hit zero (see PLAN.md §4). Enemies blow up and are
    // removed; the player blows up and respawns (lives + invuln frames come in later
    // phases). The damage itself is applied upstream by the CollisionSystem.
    public class DamageSystem : EntityProcessingSystem
    {
        // Set by Game1 after the World is built (factory-wiring note in AGENTS.md).
        public EntityFactory Factory;
        public AudioManager Audio;

        // Invulnerability granted on respawn so the player isn't instantly re-killed.
        private const float RespawnInvuln = 2.5f;

        private ComponentMapper<Health> _healthMapper;
        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<Player> _playerMapper;

        public DamageSystem() : base(Aspect.All(typeof(Health), typeof(Transform))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _healthMapper = mapperService.GetMapper<Health>();
            _transformMapper = mapperService.GetMapper<Transform>();
            _playerMapper = mapperService.GetMapper<Player>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var health = _healthMapper.Get(entityId);
            if (health.Current > 0)
                return;

            var transform = _transformMapper.Get(entityId);
            Factory.CreateExplosion(transform.Position);

            if (_playerMapper.Has(entityId))
            {
                // Player death: respawn at the start position, restore health, grant i-frames.
                Audio?.Play("audio/sfx/sfx_player_explode");
                transform.Position = EntityFactory.PlayerSpawn;
                health.Current = health.Max;
                _playerMapper.Get(entityId).InvulnTimer = RespawnInvuln;
            }
            else
            {
                Audio?.Play("audio/sfx/sfx_enemy_explode");
                DestroyEntity(entityId);
            }
        }
    }
}
