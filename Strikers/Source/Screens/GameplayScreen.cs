using Strikers.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Screens;

namespace Strikers.Screens
{
    public class GameplayScreen : GameScreen
    {
        private readonly Game1 _game;
        private World _world;
        private GameState _gameState;
        private GamePhase _lastPhase;
        private bool _confirmHeld;
        private bool _pauseHeld;

        public GameplayScreen(Game1 game) : base(game)
        {
            _game = game;
            game.IsMouseVisible = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            NewGame();
        }

        public override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            var pad = GamePad.GetState(PlayerIndex.One);

            if (pad.Buttons.Back == ButtonState.Pressed)
                Game.Exit();

            bool pause = kb.IsKeyDown(Keys.Escape) ||
                         (pad.IsConnected && pad.Buttons.Start == ButtonState.Pressed);
            if (pause && !_pauseHeld &&
                (_gameState.Phase == GamePhase.Playing || _gameState.Phase == GamePhase.Paused))
            {
                _gameState.Phase = _gameState.Phase == GamePhase.Playing
                    ? GamePhase.Paused
                    : GamePhase.Playing;
            }
            _pauseHeld = pause;

            bool confirm = kb.IsKeyDown(Keys.Enter) ||
                           (pad.IsConnected && pad.Buttons.Start == ButtonState.Pressed);
            if ((_gameState.Phase == GamePhase.StageClear || _gameState.Phase == GamePhase.GameOver) &&
                confirm && !_confirmHeld)
            {
                NewGame();
            }
            _confirmHeld = confirm;

            _world.Update(gameTime);
            HandlePhaseMusic();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _world.Draw(gameTime);
        }

        // Builds a fresh world for a run and wires spawning/reacting systems with the
        // factory + shared services. The factory needs the built World, so we build first
        // then inject (see AGENTS.md).
        private void NewGame()
        {
            _gameState = new GameState();
            _lastPhase = _gameState.Phase;
            var stage = Stage.CreateDefault();
            var tracker = new PlayerTracker();

            var inputSystem = new InputSystem { State = _gameState };
            var playerControlSystem = new PlayerControlSystem { Tracker = tracker, State = _gameState };
            var weaponSystem = new WeaponSystem { State = _gameState };
            var bombSystem = new BombSystem { State = _gameState };
            var enemySpawnSystem = new EnemySpawnSystem { Stage = stage, State = _gameState };
            var emitterSystem = new EmitterSystem { State = _gameState };
            var movementSystem = new MovementSystem { State = _gameState };
            var backgroundScrollSystem = new BackgroundScrollSystem { State = _gameState };
            var collisionSystem = new CollisionSystem { State = _gameState };
            var damageSystem = new DamageSystem { State = _gameState };
            var lifetimeSystem = new LifetimeSystem { State = _gameState };
            var hudSystem = new HudSystem(_game.SpriteBatch, _game.Font, _game.LifeIcon, _game.BombIcon)
            {
                State = _gameState
            };

            _world = new WorldBuilder()
                .AddSystem(inputSystem)
                .AddSystem(playerControlSystem)
                .AddSystem(bombSystem)
                .AddSystem(weaponSystem)
                .AddSystem(enemySpawnSystem)
                .AddSystem(emitterSystem)
                .AddSystem(movementSystem)
                .AddSystem(backgroundScrollSystem)
                .AddSystem(collisionSystem)
                .AddSystem(damageSystem)
                .AddSystem(lifetimeSystem)
                .AddSystem(new AnimationSystem(_game.Animations))
                .AddSystem(new RenderSystem(_game.SpriteBatch))
                .AddSystem(hudSystem)
                .Build();

            var factory = new EntityFactory(_world, Content, _game.Animations);
            weaponSystem.Factory = factory;
            weaponSystem.Sfx = _game.Sfx;
            bombSystem.Sfx = _game.Sfx;
            enemySpawnSystem.Factory = factory;
            enemySpawnSystem.Sfx = _game.Sfx;
            emitterSystem.Factory = factory;
            emitterSystem.Sfx = _game.Sfx;
            emitterSystem.Tracker = tracker;
            collisionSystem.Sfx = _game.Sfx;
            damageSystem.Factory = factory;
            damageSystem.Sfx = _game.Sfx;

            factory.CreateBackground();
            factory.CreatePlayer(EntityFactory.PlayerSpawn);

            _game.Music.Play(Assets.Music.Stage);
        }

        private void HandlePhaseMusic()
        {
            if (_gameState.Phase == _lastPhase)
                return;

            if (_gameState.Phase == GamePhase.StageClear)
                _game.Music.Stop();
            else if (_gameState.Phase == GamePhase.GameOver)
                _game.Music.Play(Assets.Music.GameOver, loop: false);

            _lastPhase = _gameState.Phase;
        }
    }
}
