using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class GameState : SceneComponent
    {
        public int Score { get; private set; }
        public int HighScore { get; set; }
        public int Lives { get; private set; }
        public int Wave { get; set; }
        public bool IsGameOver { get; private set; }
        public bool ExtraLifeAwarded { get; private set; }
        SoundEffect _extraLife;

        public GameState()
        {
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
            if (Score > HighScore)
                HighScore = Score;

            if (!ExtraLifeAwarded && Score >= Constants.ExtraLifeScore)
            {
                ExtraLifeAwarded = true;
                Lives++;
                _extraLife.Play();
            }
        }

        public void LoseLife()
        {
            Lives--;
            if (Lives <= 0)
                IsGameOver = true;
        }

        public void TriggerGameOver()
        {
            IsGameOver = true;
        }

        public void Reset()
        {
            Score = 0;
            Lives = Constants.StartingLives;
            Wave = 1;
            IsGameOver = false;
            ExtraLifeAwarded = false;
        }
    }
}
