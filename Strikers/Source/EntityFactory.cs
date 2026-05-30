using System;
using Extended.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;

namespace Extended
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

        private readonly Texture2D _shipTexture;
        private readonly Texture2D _bulletTexture;
        private readonly Texture2D _roundBulletTexture;
        private readonly Texture2D _needleBulletTexture;
        private readonly Texture2D _popcornTexture;
        private readonly Texture2D _fighterTexture;
        private readonly Texture2D _gunshipTexture;

        public EntityFactory(World world, ContentManager content, AnimationLibrary animations)
        {
            _world = world;
            _animations = animations;
            _shipTexture = content.Load<Texture2D>("sprites/player/player_ship");
            _bulletTexture = content.Load<Texture2D>("sprites/bullets/bullet_player");
            _roundBulletTexture = content.Load<Texture2D>("sprites/bullets/bullet_enemy_round");
            _needleBulletTexture = content.Load<Texture2D>("sprites/bullets/bullet_enemy_needle");
            _popcornTexture = content.Load<Texture2D>("sprites/enemies/enemy_popcorn");
            _fighterTexture = content.Load<Texture2D>("sprites/enemies/enemy_fighter");
            _gunshipTexture = content.Load<Texture2D>("sprites/enemies/enemy_gunship");
        }

        public Entity CreatePlayer(Vector2 position)
        {
            var entity = _world.CreateEntity();
            entity.Attach(new Transform(position));
            // A couple of seconds of i-frames so the player isn't killed the instant they spawn.
            entity.Attach(new Player(speed: 300f) { InvulnTimer = 2f });
            entity.Attach(new Weapon(fireInterval: 0.15f, bulletSpeed: 600f, damage: 1));
            entity.Attach(new Sprite(_shipTexture, new Vector2(48, 48), layerDepth: 0.5f));
            entity.Attach(new Health(1));
            // Tiny hitbox at the ship's center — bullet-hell fair (see PLAN.md §4).
            entity.Attach(new CircleCollider(4f, CollisionLayer.Player,
                CollisionLayer.Enemy | CollisionLayer.EnemyBullet));
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
                EnemyType.Fighter => (_fighterTexture, 40f, 4, 200),
                _                 => (_gunshipTexture, 64f, 10, 500),
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
                        fireInterval: 1.1f, bulletSpeed: 260f, damage: 1));
                    break;
                case EnemyType.Gunship:
                    entity.Attach(new Emitter(BulletPattern.Spiral, BulletKind.Round,
                        fireInterval: 0.16f, bulletSpeed: 130f, damage: 1, bulletCount: 3, spinRate: 0.36f));
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
    }
}
