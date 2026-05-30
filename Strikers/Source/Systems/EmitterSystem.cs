using System;
using Extended.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Extended.Systems
{
    // Drives enemy bullet emitters: counts down each Emitter's cooldown and, when it
    // fires, spawns a volley shaped entirely by the emitter's data — aimed, spread,
    // ring, or spiral (see PLAN.md §4). Patterns are parameters, not per-enemy code.
    // Aimed shots read the player's position from the shared PlayerTracker.
    public class EmitterSystem : EntityProcessingSystem
    {
        // Set by Game1 after the World is built (factory-wiring note in AGENTS.md).
        public EntityFactory Factory;
        public AudioManager Audio;
        public PlayerTracker Tracker;

        private ComponentMapper<Emitter> _emitterMapper;
        private ComponentMapper<Transform> _transformMapper;

        public EmitterSystem() : base(Aspect.All(typeof(Emitter), typeof(Transform))) { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _emitterMapper = mapperService.GetMapper<Emitter>();
            _transformMapper = mapperService.GetMapper<Transform>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var emitter = _emitterMapper.Get(entityId);
            if (emitter.Cooldown > 0f)
                emitter.Cooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Hold fire until the enemy is actually on the playfield.
            var position = _transformMapper.Get(entityId).Position;
            if (position.Y < 0f || position.Y > VirtualResolution.Height)
                return;
            if (emitter.Cooldown > 0f)
                return;

            emitter.Cooldown = emitter.FireInterval;
            FireVolley(position, emitter);
            Audio?.Play("audio/sfx/sfx_enemy_shot");
        }

        private void FireVolley(Vector2 position, Emitter e)
        {
            switch (e.Pattern)
            {
                case BulletPattern.Aimed:
                    FireArc(position, AimAtPlayer(position), e.SpreadAngle, e.BulletCount, e);
                    break;
                case BulletPattern.Spread:
                    FireArc(position, e.BaseAngle, e.SpreadAngle, e.BulletCount, e);
                    break;
                case BulletPattern.Ring:
                    FireRing(position, e.BaseAngle, e.BulletCount, e);
                    break;
                case BulletPattern.Spiral:
                    FireRing(position, e.SpinAngle, e.BulletCount, e);
                    e.SpinAngle += e.SpinRate;
                    break;
            }
        }

        // `count` bullets fanned evenly across `arc` radians, centered on `baseAngle`.
        private void FireArc(Vector2 pos, float baseAngle, float arc, int count, Emitter e)
        {
            if (count <= 1)
            {
                Fire(pos, baseAngle, e);
                return;
            }
            float start = baseAngle - arc * 0.5f;
            float step = arc / (count - 1);
            for (int i = 0; i < count; i++)
                Fire(pos, start + step * i, e);
        }

        // `count` bullets spaced evenly around the full circle from `baseAngle`.
        private void FireRing(Vector2 pos, float baseAngle, int count, Emitter e)
        {
            if (count < 1) count = 1;
            float step = MathHelper.TwoPi / count;
            for (int i = 0; i < count; i++)
                Fire(pos, baseAngle + step * i, e);
        }

        private void Fire(Vector2 pos, float angle, Emitter e)
        {
            var velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * e.BulletSpeed;
            Factory.CreateEnemyBullet(pos, velocity, e.Damage, e.Kind);
        }

        private float AimAtPlayer(Vector2 from)
        {
            if (Tracker == null || !Tracker.HasPlayer)
                return MathHelper.PiOver2; // straight down if the player isn't known yet
            var d = Tracker.Position - from;
            return MathF.Atan2(d.Y, d.X);
        }
    }
}
