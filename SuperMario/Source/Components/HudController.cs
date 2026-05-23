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
            _gameState.LivesChanged += OnLivesChanged;
        }

        public override void OnRemovedFromEntity()
        {
            _gameState.LivesChanged -= OnLivesChanged;
        }

        void OnLivesChanged(int lives) => UpdateText();

        void UpdateText() => _text.SetText($"{_level.Name}    Lives: {_gameState.Lives}");
    }
}
