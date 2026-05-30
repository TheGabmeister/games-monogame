using Strikers.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Plays back the stage's wave timeline (see Stage / PLAN.md §5): it advances a clock
    // while the run is live and spawns every wave entry whose time has come, in order.
    // Once the whole timeline has fired and the playfield is empty of enemies, the stage
    // is cleared and the GameState flips to STAGE CLEAR. The timeline itself is the plain
    // Stage class — this system just reads from it (ECS-where-it-earns-its-keep, §6).
    public class EnemySpawnSystem : EntityUpdateSystem
    {
        // Set by Game1 after the World is built (factory-wiring note in AGENTS.md).
        public EntityFactory Factory;
        public Stage Stage;
        public GameState State;
        public SfxManager Sfx;

        private float _elapsed;
        private int _index;

        // Aspect is the live-enemy set, so ActiveEntities.Count tells us when the field
        // is clear for the STAGE CLEAR check.
        public EnemySpawnSystem() : base(Aspect.All(typeof(Enemy))) { }

        public override void Initialize(IComponentMapperService mapperService) { }

        public override void Update(GameTime gameTime)
        {
            if (State == null || State.Phase != GamePhase.Playing || Stage == null)
                return;

            _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

            var spawns = Stage.Spawns;
            while (_index < spawns.Count && spawns[_index].Time <= _elapsed)
            {
                var s = spawns[_index++];
                Factory.CreateEnemy(s.Type, s.Position, s.Velocity);
            }

            // Whole timeline fired and nothing left alive → the stage is won.
            if (_index >= spawns.Count && ActiveEntities.Count == 0)
            {
                State.Phase = GamePhase.StageClear;
                Sfx?.Play(Assets.Sfx.StageClear);
            }
        }
    }
}
