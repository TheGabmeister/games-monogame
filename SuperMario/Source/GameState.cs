using System;

namespace SuperMario
{
    public class GameState
    {
        int _lives = Constants.StartingLives;

        public int Score { get; set; }
        public PlayerState PowerState { get; set; } = PlayerState.Small;

        public int Lives
        {
            get => _lives;
            set
            {
                if (_lives == value) return;
                _lives = value;
                LivesChanged?.Invoke(_lives);
            }
        }

        public event Action<int> LivesChanged;
    }
}
