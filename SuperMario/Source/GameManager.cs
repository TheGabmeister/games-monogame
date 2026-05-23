using Nez;

namespace SuperMario
{
    public class GameManager : GlobalManager
    {
        GameState _gameState;

        public GameState State => _gameState;

        public override void OnEnabled()
        {
            LoadMainMenu();
        }

        public void LoadMainMenu()
        {
            var menu = new MainMenuScene();
            menu.StartPressed += StartGame;
            Core.Scene = menu;
        }

        public void StartGame()
        {
            _gameState = new GameState();
            LoadLevel(Levels.World1_1);
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
