using System;

namespace SuperMario
{
    public class GameState
    {
        const int MaxScore = 999999;

        int _lives = Constants.StartingLives;
        int _score;

        public int Score
        {
            get => _score;
            private set
            {
                var clamped = Math.Min(value, MaxScore);
                if (_score == clamped) return;
                _score = clamped;
                ScoreChanged?.Invoke(_score);
            }
        }

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

        public void AddScore(int points)
        {
            if (points <= 0) return;
            Score += points;
        }

        public event Action<int> ScoreChanged;
        public event Action<int> LivesChanged;
    }
}
