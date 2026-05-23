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
            Audio.PlayMusic(_level.MusicPath);

            _factory = new EntityFactory(_gameState);
            var map = Content.LoadTiledMap(_level.MapPath);
            var objects = map.GetObjectGroup("entities");

            foreach (var obj in objects.Objects)
                _factory.Spawn(this, obj);

            SpawnCleanupVolume(map.WorldWidth, map.WorldHeight);
            SpawnPlayer();
            SpawnHud();
        }

        void SpawnCleanupVolume(int mapWidth, int mapHeight)
        {
            const float cleanupOffset = 256f;
            const float cleanupHeight = 128f;

            var cleanupWidth = mapWidth + Constants.ScreenWidth * 2f;
            var cleanupPosition = new Vector2(mapWidth / 2f, mapHeight + cleanupOffset);
            var cleanup = CreateEntity("cleanupvolume", cleanupPosition);

            var collider = cleanup.AddComponent(new BoxCollider(
                -cleanupWidth / 2f,
                -cleanupHeight / 2f,
                cleanupWidth,
                cleanupHeight));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers =
                (1 << PhysicsLayers.Player) |
                (1 << PhysicsLayers.Enemy) |
                (1 << PhysicsLayers.PickupBody) |
                (1 << PhysicsLayers.Projectile);
            collider.IsTrigger = true;

            cleanup.AddComponent(new CleanupVolume());
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
            _playerController = PlayerController.Spawn(this, position);
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
