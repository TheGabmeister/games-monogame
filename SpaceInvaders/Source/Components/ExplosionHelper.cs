using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Particles;
using Nez.Textures;

namespace SpaceInvaders
{
    public static class ExplosionHelper
    {
        public static void Spawn(Scene scene, Vector2 position, int particleCount, float lifespan, Color startColor, Color finishColor, float startSize = 1f, float finishSize = 0f)
        {
            var texture = scene.Content.LoadTexture(Assets.Sprites.Effects.ExplosionParticle, true);

            var config = new ParticleEmitterConfig
            {
                Sprite = new Sprite(texture),
                MaxParticles = (uint)particleCount,
                EmissionRate = particleCount / 0.01f,
                Duration = 0.01f,
                ParticleLifespan = lifespan,
                ParticleLifespanVariance = lifespan * 0.3f,
                Speed = 120f,
                SpeedVariance = 40f,
                Angle = 0f,
                AngleVariance = 360f,
                StartColor = startColor,
                FinishColor = finishColor,
                StartParticleSize = startSize,
                StartParticleSizeVariance = startSize * 0.3f,
                FinishParticleSize = finishSize,
                SimulateInWorldSpace = true,
                BlendFuncSource = Blend.SourceAlpha,
                BlendFuncDestination = Blend.One
            };

            var entity = scene.CreateEntity("explosion", position);
            var emitter = entity.AddComponent(new ParticleEmitter(config));
            emitter.OnAllParticlesExpired += _ => entity.Destroy();
        }

        public static void SpawnInvaderExplosion(Scene scene, Vector2 position, InvaderType type)
        {
            var color = type switch
            {
                InvaderType.Octopus => Color.LimeGreen,
                InvaderType.Crab => Color.CornflowerBlue,
                InvaderType.Squid => Color.MediumPurple,
                _ => Color.White
            };
            Spawn(scene, position, 10, 0.3f, color, Color.Transparent, 0.8f);
        }

        public static void SpawnPlayerExplosion(Scene scene, Vector2 position)
        {
            Spawn(scene, position, 18, 0.5f, Color.White, Color.Yellow * 0f, 1.2f);
        }

        public static void SpawnUfoExplosion(Scene scene, Vector2 position)
        {
            Spawn(scene, position, 12, 0.4f, Color.OrangeRed, Color.Transparent, 1f);
        }

        public static void SpawnShieldPuff(Scene scene, Vector2 position)
        {
            Spawn(scene, position, 4, 0.15f, Color.LimeGreen, Color.Transparent, 0.4f, 0f);
        }
    }
}
