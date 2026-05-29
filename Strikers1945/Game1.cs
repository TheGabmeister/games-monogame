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

            // WeaponSystem needs the EntityFactory, but the factory needs the World,
            // which only exists after Build(). So build the world first, then inject
            // the factory into the systems that spawn entities.
            var weaponSystem = new WeaponSystem();

            _world = new WorldBuilder()
                .AddSystem(new InputSystem())
                .AddSystem(new PlayerControlSystem())
                .AddSystem(weaponSystem)
                .AddSystem(new MovementSystem())
                .AddSystem(new LifetimeSystem())
                .AddSystem(new RenderSystem(_spriteBatch))
                .Build();

            var factory = new EntityFactory(_world, Content);
            weaponSystem.Factory = factory;
            weaponSystem.Audio = new AudioManager(Content);

            factory.CreatePlayer(
                new Vector2(VirtualResolution.Width / 2f, VirtualResolution.Height - 100f));
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
