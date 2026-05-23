using Nez;

namespace SuperMario
{
    public class HudController : Component
    {
        readonly GameState _gameState;
        TextComponent _text;

        public HudController(GameState gameState)
        {
            _gameState = gameState;
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

        void UpdateText() => _text.SetText($"Lives: {_gameState.Lives}");
    }
}
