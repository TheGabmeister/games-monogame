using Strikers.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;

namespace Strikers
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private World _world;

        // Resources that outlive a single run, loaded once and reused when the stage is
        // restarted (textures/sounds are cached by the ContentManager anyway).
        private AnimationLibrary _animations;
        private AudioManager _audio;
        private SpriteFont _font;
        private Texture2D _lifeIcon;
        private Texture2D _bombIcon;

        // Global arcade flow + score (see PLAN.md §6). Recreated for each new run.
        private GameState _gameState;
        private bool _confirmHeld; // edge-detect the restart button

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

            _animations = new AnimationLibrary(Content);
            _animations.Load();
            _audio = new AudioManager(Content);
            _font = Content.Load<SpriteFont>(Assets.Fonts.Main);
            _lifeIcon = Content.Load<Texture2D>(Assets.Sprites.HudLifeIcon);
            _bombIcon = Content.Load<Texture2D>(Assets.Sprites.HudBombIcon);

            NewGame();
        }

        // Builds a fresh world for a run and wires the spawning/reacting systems with the
        // factory + shared services. The factory needs the built World, so we build first
        // then inject (see AGENTS.md). Called at startup and on restart after the run ends.
        private void NewGame()
        {
            _gameState = new GameState();
            var stage = Stage.CreateDefault();
            var tracker = new PlayerTracker();

            var playerControlSystem = new PlayerControlSystem { Tracker = tracker, State = _gameState };
            var weaponSystem = new WeaponSystem { State = _gameState };
            var bombSystem = new BombSystem { State = _gameState };
            var enemySpawnSystem = new EnemySpawnSystem { Stage = stage, State = _gameState };
            var emitterSystem = new EmitterSystem { State = _gameState };
            var collisionSystem = new CollisionSystem { State = _gameState };
            var damageSystem = new DamageSystem { State = _gameState };
            var hudSystem = new HudSystem(_spriteBatch, _font, _lifeIcon, _bombIcon) { State = _gameState };

            _world = new WorldBuilder()
                .AddSystem(new InputSystem())
                .AddSystem(playerControlSystem)
                .AddSystem(bombSystem)
                .AddSystem(weaponSystem)
                .AddSystem(enemySpawnSystem)
                .AddSystem(emitterSystem)
                .AddSystem(new MovementSystem())
                .AddSystem(new BackgroundScrollSystem())
                .AddSystem(collisionSystem)
                .AddSystem(damageSystem)
                .AddSystem(new LifetimeSystem())
                .AddSystem(new AnimationSystem(_animations))
                .AddSystem(new RenderSystem(_spriteBatch))
                .AddSystem(hudSystem)
                .Build();

            var factory = new EntityFactory(_world, Content, _animations);
            weaponSystem.Factory = factory;
            weaponSystem.Audio = _audio;
            bombSystem.Audio = _audio;
            enemySpawnSystem.Factory = factory;
            enemySpawnSystem.Audio = _audio;
            emitterSystem.Factory = factory;
            emitterSystem.Audio = _audio;
            emitterSystem.Tracker = tracker;
            collisionSystem.Audio = _audio;
            damageSystem.Factory = factory;
            damageSystem.Audio = _audio;

            factory.CreateBackground();
            factory.CreatePlayer(EntityFactory.PlayerSpawn);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // After STAGE CLEAR / GAME OVER, the confirm button starts a fresh run.
            bool confirm = Keyboard.GetState().IsKeyDown(Keys.Enter) ||
                           GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed;
            if (_gameState.Phase != GamePhase.Playing && confirm && !_confirmHeld)
                NewGame();
            _confirmHeld = confirm;

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
