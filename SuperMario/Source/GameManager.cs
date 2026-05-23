using Nez;

namespace SuperMario
{
    public class GameManager : GlobalManager
    {
        readonly LevelDefinition[] _campaign =
        {
            Levels.World1_1,
            Levels.World1_2,
            Levels.World1_3,
        };

        GameState _gameState;
        int _currentLevelIndex;

        public GameState State => _gameState;

        public override void OnEnabled()
        {
            // Skip to StartGame when debugging
            //LoadMainMenu(); 
            StartGame();
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
            _currentLevelIndex = 0;
            LoadLevel(_campaign[_currentLevelIndex]);
        }

        public void LoadLevel(LevelDefinition level)
        {
            var scene = new GameplayScene(level, _gameState);
            scene.LevelCompleted += () => OnLevelCompleted(level);
            scene.PlayerDied += () => OnPlayerDied(level);
            Core.Scene = scene;
        }

        void OnLevelCompleted(LevelDefinition currentLevel)
        {
            _currentLevelIndex++;

            if (_currentLevelIndex >= _campaign.Length)
            {
                LoadGameOver();
                return;
            }

            LoadLevel(_campaign[_currentLevelIndex]);
        }

        void OnPlayerDied(LevelDefinition level)
        {
            _gameState.PowerState = PlayerState.Small;
            _gameState.Lives--;

            if (_gameState.Lives <= 0)
            {
                LoadGameOver();
                return;
            }

            LoadLevel(level);
        }

        void LoadGameOver()
        {
            var gameOver = new GameOverScene();
            gameOver.Continue += LoadMainMenu;
            Core.Scene = gameOver;
        }
    }
}
