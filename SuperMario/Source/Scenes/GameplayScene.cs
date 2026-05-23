using System;
using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class GameplayScene : Scene
    {
        readonly LevelDefinition _level;
        readonly GameState _gameState;
        EntityFactory _factory;
        PlayerController _playerController;
        bool _levelCompleted;

        public event Action LevelCompleted;
        public event Action PlayerDied;

        public GameplayScene(LevelDefinition level, GameState gameState)
        {
            _level = level;
            _gameState = gameState;
        }

        public override void Initialize()
        {
            ClearColor = Color.CornflowerBlue;

            SetDesignResolution(
                Constants.ScreenWidth,
                Constants.ScreenHeight,
                SceneResolutionPolicy.ShowAllPixelPerfect);

            AddRenderer(new RenderLayerExcludeRenderer(0, RenderLayers.Hud));
            AddRenderer(new ScreenSpaceRenderer(1, RenderLayers.Hud));
        }

        public override void OnStart()
        {
            Core.GetGlobalManager<MusicManager>().Play(_level.MusicPath);

            _factory = new EntityFactory(_gameState);
            var map = Content.LoadTiledMap(_level.MapPath);
            var objects = map.GetObjectGroup("entities");

            foreach (var obj in objects.Objects)
                _factory.Spawn(this, obj);

            SpawnPlayer();
            SpawnHud();
        }

        void SpawnHud()
        {
            var hud = CreateEntity("hud", new Vector2(16, 16));
            hud.Scale = new Vector2(2);
            hud.AddComponent(new TextComponent()).SetRenderLayer(RenderLayers.Hud);
            hud.AddComponent(new HudController(_gameState, _level));
        }

        void SpawnPlayer()
        {
            var start = FindComponentOfType<PlayerStart>();
            if (start == null)
                throw new Exception("Level is missing a PlayerStart object.");

            var position = start.Entity.Position;
            _playerController = _factory.CreatePlayer(this, position);
            _playerController.SetGameState(_gameState);
            _playerController.OnDied += OnPlayerDied;
        }

        public void CompleteLevel()
        {
            if (_levelCompleted)
                return;
            
            // TODO:
            // Play level complete sound
            // Do an animation
            // Spawn particle effects

            _levelCompleted = true;



            LevelCompleted?.Invoke();
        }

        void OnPlayerDied()
        {
            // TODO:
            // Pause music
            // pause the game for a few seconds
            PlayerDied?.Invoke();
        }
    }
}
