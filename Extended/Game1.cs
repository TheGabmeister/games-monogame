using Extended.Components;
using Extended.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;

namespace Extended
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private World _world;
        private Entity _playerEntity;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _world = new WorldBuilder()
                .AddSystem(new PlayerSystem())
                .AddSystem(new RenderSystem(_spriteBatch))
                .Build();

            // A 1x1 white pixel stands in for a sprite so this runs with no content assets.
            // To use a real sprite instead: _playerEntity.Attach(Content.Load<Texture2D>("your-asset"));
            var pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            _playerEntity = _world.CreateEntity();
            _playerEntity.Attach(pixel);
            _playerEntity.Attach(new Player(100,
                new Vector2(GraphicsDevice.Viewport.Width / 2,
                            GraphicsDevice.Viewport.Height / 2)));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
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
