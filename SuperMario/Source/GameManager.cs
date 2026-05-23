using Nez;

namespace SuperMario
{
    public class GameManager : GlobalManager
    {
        GameState _gameState;

        public GameState State => _gameState;

        public override void OnEnabled()
        {
            StartGame();
        }

        public void StartGame()
        {
            _gameState = new GameState();
            LoadLevel(Assets.Maps.Debug);
        }

        public void LoadLevel(string levelPath)
        {
            var scene = new GameplayScene(levelPath, _gameState);
            scene.LevelCompleted += () => OnLevelCompleted(levelPath);
            scene.GameOver += OnGameOver;
            Core.Scene = scene;
        }

        void OnLevelCompleted(string currentLevel)
        {
            // TODO: determine next level and load it
            // LoadLevel(Assets.Maps.Level2);
        }

        void OnGameOver()
        {
            StartGame();
        }
    }
}
