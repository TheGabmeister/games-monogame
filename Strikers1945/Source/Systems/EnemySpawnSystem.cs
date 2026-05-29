using System;
using Extended.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Extended.Systems
{
    // A simple timed spawner: every Interval seconds it drops one enemy in at the top
    // of the playfield, cycling through the archetypes, up to a concurrent cap. This is
    // the Phase 2 stand-in; the full wave/timeline scheduler arrives in Phase 4 (see
    // PLAN.md §5). Enemy motion is plain downward Velocity, integrated by MovementSystem.
    public class EnemySpawnSystem : EntityUpdateSystem
    {
        // Set by Game1 after the World is built (see the factory-wiring note in AGENTS.md).
        public EntityFactory Factory;

        private const float Interval = 1.1f;
        private const int MaxConcurrent = 12;
        private const float Margin = 40f;

        private readonly Random _rng = new();
        private float _timer;
        private int _cycle;

        public EnemySpawnSystem() : base(Aspect.All(typeof(Enemy))) { }

        public override void Initialize(IComponentMapperService mapperService) { }

        public override void Update(GameTime gameTime)
        {
            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer > 0f || ActiveEntities.Count >= MaxConcurrent)
                return;

            _timer = Interval;

            var type = (EnemyType)(_cycle++ % 3);
            var x = MathHelper.Lerp(Margin, VirtualResolution.Width - Margin, (float)_rng.NextDouble());
            var speed = _rng.Next(110, 170);

            Factory.CreateEnemy(type, new Vector2(x, -Margin), new Vector2(0f, speed));
        }
    }
}
