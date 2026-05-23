using Nez;

namespace SuperMario
{
    public class Game1 : Core
    {
        GameState _gameState;

        public Game1()
            : base(Constants.ScreenWidth, Constants.ScreenHeight, false, "SuperMario")
        {
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            DebugRenderEnabled = true;

            StartGame();
        }

        void StartGame()
        {
            _gameState = new GameState();
            LoadLevel(Assets.Maps.Debug);
        }

        void LoadLevel(string levelPath)
        {
            var scene = new GameplayScene(levelPath, _gameState);
            scene.LevelCompleted += () => OnLevelCompleted(levelPath);
            scene.GameOver += OnGameOver;
            Scene = scene;
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
