using Extended.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;

namespace Extended
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private World _world;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = VirtualResolution.Width,
                PreferredBackBufferHeight = VirtualResolution.Height,
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var animations = new AnimationLibrary(Content);
            animations.Load();
            var audio = new AudioManager(Content);
            var tracker = new PlayerTracker();

            // Spawning/reacting systems need the EntityFactory, but the factory needs the
            // World, which only exists after Build(). So build the world first, then inject
            // the factory and shared services into the systems that need them (see AGENTS.md).
            var playerControlSystem = new PlayerControlSystem { Tracker = tracker };
            var weaponSystem = new WeaponSystem();
            var enemySpawnSystem = new EnemySpawnSystem();
            var emitterSystem = new EmitterSystem();
            var collisionSystem = new CollisionSystem();
            var damageSystem = new DamageSystem();

            _world = new WorldBuilder()
                .AddSystem(new InputSystem())
                .AddSystem(playerControlSystem)
                .AddSystem(weaponSystem)
                .AddSystem(enemySpawnSystem)
                .AddSystem(emitterSystem)
                .AddSystem(new MovementSystem())
                .AddSystem(collisionSystem)
                .AddSystem(damageSystem)
                .AddSystem(new LifetimeSystem())
                .AddSystem(new AnimationSystem(animations))
                .AddSystem(new RenderSystem(_spriteBatch))
                .Build();

            var factory = new EntityFactory(_world, Content, animations);
            weaponSystem.Factory = factory;
            weaponSystem.Audio = audio;
            enemySpawnSystem.Factory = factory;
            emitterSystem.Factory = factory;
            emitterSystem.Audio = audio;
            emitterSystem.Tracker = tracker;
            collisionSystem.Audio = audio;
            damageSystem.Factory = factory;
            damageSystem.Audio = audio;

            factory.CreatePlayer(EntityFactory.PlayerSpawn);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _world.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _world.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
