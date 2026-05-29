using Extended.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;

namespace Extended
{
    // Single place that assembles configured entities from components, so systems
    // never new-up components inline. Grows a Create* method per entity kind
    // (CreateEnemy, CreatePowerUp) in later phases. Textures are pulled from the
    // ContentManager, which caches, so repeated loads are cheap.
    public class EntityFactory
    {
        private readonly World _world;
        private readonly Texture2D _shipTexture;
        private readonly Texture2D _bulletTexture;

        public EntityFactory(World world, ContentManager content)
        {
            _world = world;
            _shipTexture = content.Load<Texture2D>("sprites/player/player_ship");
            _bulletTexture = content.Load<Texture2D>("sprites/bullets/bullet_player");
        }

        public Entity CreatePlayer(Vector2 position)
        {
            var entity = _world.CreateEntity();
            entity.Attach(new Transform(position));
            entity.Attach(new Player(speed: 300f));
            entity.Attach(new Weapon(fireInterval: 0.15f, bulletSpeed: 600f, damage: 1));
            entity.Attach(new Sprite(_shipTexture, new Vector2(48, 48), layerDepth: 0.5f));
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
            return entity;
        }
    }
}
