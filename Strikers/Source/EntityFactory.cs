using System;
using Strikers.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;

namespace Strikers
{
    // Single place that assembles configured entities from components, so systems never
    // new-up components inline. Grows a Create* method per entity kind as phases land.
    // Textures are pulled from the ContentManager, which caches, so repeated loads are cheap.
    public class EntityFactory
    {
        // Where the player starts and respawns. Used by Game1 (initial spawn) and the
        // DamageSystem (respawn after death), so both agree on one location.
        public static Vector2 PlayerSpawn =>
            new(VirtualResolution.Width / 2f, VirtualResolution.Height - 100f);

        private readonly World _world;
        private readonly AnimationLibrary _animations;

        // Player arcade starting stock (spare ships + bombs). Weapon starts at level 1.
        public const int StartingLives = 2;
        public const int StartingBombs = 2;
        public const int MaxWeaponLevel = 4;
        public const int MaxBombs = 6;

        private readonly Texture2D _shipTexture;
        private readonly Texture2D _sparkTexture;
        private readonly Texture2D _bulletTexture;
        private readonly Texture2D _roundBulletTexture;
        private readonly Texture2D _needleBulletTexture;
        private readonly Texture2D _popcornTexture;
        private readonly Texture2D _fighterTexture;
        private readonly Texture2D _gunshipTexture;
        private readonly Texture2D _bgTexture;
        private readonly Texture2D _powerWeaponTexture;
        private readonly Texture2D _powerBombTexture;
        private readonly Texture2D _powerScoreTexture;

        public EntityFactory(World world, ContentManager content, AnimationLibrary animations)
        {
            _world = world;
            _animations = animations;
            _shipTexture = content.Load<Texture2D>(Assets.Sprites.PlayerShip);
            _bulletTexture = content.Load<Texture2D>(Assets.Sprites.BulletPlayer);
            _sparkTexture = content.Load<Texture2D>(Assets.Sprites.BulletEnemyRound);
            _roundBulletTexture = content.Load<Texture2D>(Assets.Sprites.BulletEnemyRound);
            _needleBulletTexture = content.Load<Texture2D>(Assets.Sprites.BulletEnemyNeedle);
            _popcornTexture = content.Load<Texture2D>(Assets.Sprites.EnemyPopcorn);
            _fighterTexture = content.Load<Texture2D>(Assets.Sprites.EnemyFighter);
            _gunshipTexture = content.Load<Texture2D>(Assets.Sprites.EnemyGunship);
            _bgTexture = content.Load<Texture2D>(Assets.Backgrounds.Tile);
            _powerWeaponTexture = content.Load<Texture2D>(Assets.Sprites.PowerUpWeapon);
            _powerBombTexture = content.Load<Texture2D>(Assets.Sprites.PowerUpBomb);
            _powerScoreTexture = content.Load<Texture2D>(Assets.Sprites.PowerUpScore);
        }

        public Entity CreatePlayer(Vector2 position)
        {
            var entity = _world.CreateEntity();
            entity.Attach(new Transform(position));
            // A couple of seconds of i-frames so the player isn't killed the instant they spawn.
            entity.Attach(new Player(speed: 300f)
            {
                InvulnTimer = 2f,
                Lives = StartingLives,
                Bombs = StartingBombs,
            });
            entity.Attach(new Weapon(fireInterval: 0.13f, bulletSpeed: 620f, damage: 1));
            entity.Attach(new Sprite(_shipTexture, new Vector2(48, 48), layerDepth: 0.5f));
            entity.Attach(new Animator("player_idle"));
            entity.Attach(new Health(1));
            // Tiny hitbox at the ship's center — bullet-hell fair (see PLAN.md §4).
            entity.Attach(new CircleCollider(4f, CollisionLayer.Player,
                CollisionLayer.Enemy | CollisionLayer.EnemyBullet | CollisionLayer.PowerUp));
            return entity;
        }

        public Entity CreateBullet(Vector2 position, Vector2 velocity, int damage)
        {
            var entity = _world.CreateEntity();
            entity.Attach(new Transform(position));
            entity.Attach(new Velocity(velocity));
            entity.Attach(new Bullet(damage));
            entity.Attach(new Lifetime(despawnWhenOffscreen: true));
            // Drawn behind the ship so the muzzle reads cleanly.
            entity.Attach(new Sprite(_bulletTexture, new Vector2(8, 16), layerDepth: 0.4f));
            entity.Attach(new CircleCollider(4f, CollisionLayer.PlayerBullet, CollisionLayer.Enemy));
            return entity;
        }

        public Entity CreateEnemy(EnemyType type, Vector2 position, Vector2 velocity)
        {
            // Per-archetype draw size, hit points, and score (see PLAN.md §7).
            var (texture, size, hp, score) = type switch
            {
                EnemyType.Popcorn => (_popcornTexture, 32f, 2, 100),
                EnemyType.Fighter => (_fighterTexture, 40f, 3, 200),
                _                 => (_gunshipTexture, 64f, 9, 500),
            };

            var entity = _world.CreateEntity();
            entity.Attach(new Transform(position));
            entity.Attach(new Velocity(velocity));
            entity.Attach(new Sprite(texture, new Vector2(size), layerDepth: 0.6f));
            entity.Attach(new Enemy(type, score));
            entity.Attach(new Health(hp));
            // Passive layer (no mask): player + player bullets mask the enemy instead.
            entity.Attach(new CircleCollider(size * 0.45f, CollisionLayer.Enemy));
            entity.Attach(new Lifetime(despawnWhenOffscreen: true));

            // Bullet patterns per archetype (see PLAN.md §7): the fighter fires aimed
            // needles, the gunship lays down a rotating spiral of round shots. Popcorn is
            // pure fodder and stays unarmed.
            switch (type)
            {
                case EnemyType.Fighter:
                    entity.Attach(new Emitter(BulletPattern.Aimed, BulletKind.Needle,
                        fireInterval: 1.25f, bulletSpeed: 245f, damage: 1));
                    break;
                case EnemyType.Gunship:
                    entity.Attach(new Emitter(BulletPattern.Spiral, BulletKind.Round,
                        fireInterval: 0.18f, bulletSpeed: 120f, damage: 1, bulletCount: 3, spinRate: 0.34f));
                    break;
            }

            return entity;
        }

        public Entity CreateEnemyBullet(Vector2 position, Vector2 velocity, int damage, BulletKind kind)
        {
            var (texture, size) = kind == BulletKind.Needle
                ? (_needleBulletTexture, new Vector2(6, 18))
                : (_roundBulletTexture, new Vector2(12, 12));

            var entity = _world.CreateEntity();
            // Point the sprite along its travel direction (art points "up" by default).
            var rotation = MathF.Atan2(velocity.Y, velocity.X) + MathHelper.PiOver2;
            entity.Attach(new Transform(position, rotation));
            entity.Attach(new Velocity(velocity));
            entity.Attach(new Bullet(damage));
            entity.Attach(new Sprite(texture, size, layerDepth: 0.45f));
            // Hitbox a touch smaller than the art so dense fire stays fair.
            entity.Attach(new CircleCollider(MathF.Min(size.X, size.Y) * 0.4f,
                CollisionLayer.EnemyBullet, CollisionLayer.Player));
            entity.Attach(new Lifetime(despawnWhenOffscreen: true));
            return entity;
        }

        public Entity CreateExplosion(Vector2 position)
        {
            var clip = _animations.Get("explosion");
            if (clip == null)
                return null;

            CreateExplosionSparks(position);

            var frame = clip.Frames[0];
            var entity = _world.CreateEntity();
            entity.Attach(new Transform(position));
            entity.Attach(new Sprite(
                clip.Texture,
                new Vector2(frame.Width, frame.Height),
                layerDepth: 0.1f,
                sourceRect: frame));
            entity.Attach(new Animator("explosion"));
            // Lives exactly as long as the one-shot clip, then despawns (see PLAN.md §5).
            entity.Attach(Lifetime.Timer(clip.Duration));
            return entity;
        }

        private void CreateExplosionSparks(Vector2 position)
        {
            const int count = 14;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                float speed = 90f + 12f * (i % 5);
                var velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;

                var spark = _world.CreateEntity();
                spark.Attach(new Transform(position));
                spark.Attach(new Velocity(velocity));
                spark.Attach(new Sprite(
                    _sparkTexture,
                    new Vector2(5f + i % 3),
                    i % 2 == 0 ? Color.Gold : Color.OrangeRed,
                    layerDepth: 0.08f));
                spark.Attach(Lifetime.Timer(0.22f + 0.03f * (i % 4)));
            }
        }

        // A power-up dropped by a dead enemy. Drifts straight down; the player picks it
        // up on contact (passive PowerUp layer — the player's collider masks it).
        public Entity CreatePowerUp(PowerUpKind kind, Vector2 position)
        {
            var texture = kind switch
            {
                PowerUpKind.Weapon => _powerWeaponTexture,
                PowerUpKind.Bomb   => _powerBombTexture,
                _                  => _powerScoreTexture,
            };

            var entity = _world.CreateEntity();
            entity.Attach(new Transform(position));
            entity.Attach(new Velocity(new Vector2(0f, 80f)));
            entity.Attach(new PowerUp(kind));
            entity.Attach(new Sprite(texture, new Vector2(24, 24), layerDepth: 0.55f));
            entity.Attach(new CircleCollider(14f, CollisionLayer.PowerUp));
            entity.Attach(new Lifetime(despawnWhenOffscreen: true));
            return entity;
        }

        // Two stacked full-screen tiles scrolling down; BackgroundScrollSystem wraps each
        // back to the top as it leaves, for a seamless loop. Drawn farthest back.
        public void CreateBackground()
        {
            var size = new Vector2(VirtualResolution.Width, VirtualResolution.Height);
            const float scroll = 60f;
            float cx = VirtualResolution.Width / 2f;
            float cy = VirtualResolution.Height / 2f;

            for (int i = 0; i < 2; i++)
            {
                var tile = _world.CreateEntity();
                // First tile fills the screen; the second sits one screen above it.
                tile.Attach(new Transform(new Vector2(cx, cy - i * VirtualResolution.Height)));
                tile.Attach(new Velocity(new Vector2(0f, scroll)));
                tile.Attach(new Sprite(_bgTexture, size, layerDepth: 0.95f));
                tile.Attach(new Background());
            }
        }
    }
}
