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
            LoadLevel(Levels.Debug);
        }

        public void LoadLevel(LevelDefinition level)
        {
            var scene = new GameplayScene(level, _gameState);
            scene.LevelCompleted += () => OnLevelCompleted(level);
            scene.GameOver += OnGameOver;
            Core.Scene = scene;
        }

        void OnLevelCompleted(LevelDefinition currentLevel)
        {
            // TODO: determine next level and load it
            // LoadLevel(Levels.World1_2);
        }

        void OnGameOver()
        {
            StartGame();
        }
    }
}
