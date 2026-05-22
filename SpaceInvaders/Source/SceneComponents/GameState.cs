using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Systems;
using System;

namespace SpaceInvaders
{
    public class GameState : SceneComponent
    {
        public event Action<int> ScoreChanged;
        public event Action<int> HighScoreChanged;
        public event Action<int> LivesChanged;
        public event Action<int> WaveChanged;
        public event Action GameOver;

        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int Lives { get; private set; }
        public int Wave { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool ExtraLifeAwarded { get; private set; }
        SoundEffect _extraLife;

        public GameState()
        {
            HighScore = Settings.Instance.HighScore;
            Lives = Constants.StartingLives;
            Wave = 1;
        }

        public override void OnEnabled()
        {
            _extraLife = Scene.Content.LoadSoundEffect(Assets.Audio.Sfx.ExtraLife);
        }

        public void AddScore(int points)
        {
            Score += points;
            ScoreChanged?.Invoke(Score);

            if (Score > HighScore)
            {
                HighScore = Score;
                HighScoreChanged?.Invoke(HighScore);
                Settings.Instance.HighScore = HighScore;
                Settings.Instance.Save();
            }

            if (!ExtraLifeAwarded && Score >= Constants.ExtraLifeScore)
            {
                ExtraLifeAwarded = true;
                Lives++;
                LivesChanged?.Invoke(Lives);
                _extraLife.Play();
            }
        }

        public void LoseLife()
        {
            Lives--;
            LivesChanged?.Invoke(Lives);

            if (Lives <= 0)
                TriggerGameOver();
        }

        public void AdvanceWave()
        {
            Wave++;
            WaveChanged?.Invoke(Wave);
        }

        public void TriggerGameOver()
        {
            if (IsGameOver)
                return;

            IsGameOver = true;
            GameOver?.Invoke();
        }

        public void Reset()
        {
            Score = 0;
            Lives = Constants.StartingLives;
            Wave = 1;
            IsGameOver = false;
            ExtraLifeAwarded = false;

            ScoreChanged?.Invoke(Score);
            HighScoreChanged?.Invoke(HighScore);
            LivesChanged?.Invoke(Lives);
            WaveChanged?.Invoke(Wave);
        }
    }
}
