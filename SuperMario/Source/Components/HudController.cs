using Nez;

namespace SuperMario
{
    public class HudController : Component
    {
        readonly GameState _gameState;
        readonly LevelDefinition _level;
        TextComponent _text;

        public HudController(GameState gameState, LevelDefinition level)
        {
            _gameState = gameState;
            _level = level;
        }

        public override void OnAddedToEntity()
        {
            _text = Entity.GetComponent<TextComponent>();
            UpdateText();
            _gameState.ScoreChanged += OnScoreChanged;
            _gameState.LivesChanged += OnLivesChanged;
        }

        public override void OnRemovedFromEntity()
        {
            _gameState.ScoreChanged -= OnScoreChanged;
            _gameState.LivesChanged -= OnLivesChanged;
        }

        void OnScoreChanged(int score) => UpdateText();

        void OnLivesChanged(int lives) => UpdateText();

        void UpdateText() => _text.SetText($"MARIO {_gameState.Score:000000}    WORLD {_level.Name}    LIVES {_gameState.Lives}");
    }
}
