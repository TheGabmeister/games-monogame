using System;
using Nez;

namespace SuperMario
{
    public class GameplayScene : Scene
    {
        EntityFactory _factory;
        PlayerController _playerController;

        public override void Initialize()
        {
            ClearColor = Microsoft.Xna.Framework.Color.CornflowerBlue;

            SetDesignResolution(
                Constants.ScreenWidth,
                Constants.ScreenHeight,
                SceneResolutionPolicy.ShowAllPixelPerfect);

            AddRenderer(new DefaultRenderer());

            _factory = new EntityFactory();
            var map = Content.LoadTiledMap(Assets.Maps.Debug);
            var objects = map.GetObjectGroup("entities");

            foreach (var obj in objects.Objects)
                _factory.Spawn(this, obj);

            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            var start = FindEntitiesWithTag(Tags.PlayerStart);
            if (start.Count == 0)
                throw new Exception("Level is missing a PlayerStart object.");

            var position = start[0].Position;
            _playerController = _factory.CreatePlayer(this, position);
            _playerController.OnDied += OnPlayerDied;
        }

        void OnPlayerDied()
        {
            _playerController.OnDied -= OnPlayerDied;
            _playerController.Entity.Destroy();
            SpawnPlayer();
        }
    }
}
