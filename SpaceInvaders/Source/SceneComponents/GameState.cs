using Nez;

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

        public GameState()
        {
            Lives = GameConstants.StartingLives;
            Wave = 1;
        }

        public void AddScore(int points)
        {
            Score += points;
            if (Score > HighScore)
                HighScore = Score;

            if (!ExtraLifeAwarded && Score >= GameConstants.ExtraLifeScore)
            {
                ExtraLifeAwarded = true;
                Lives++;
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
            Lives = GameConstants.StartingLives;
            Wave = 1;
            IsGameOver = false;
            ExtraLifeAwarded = false;
        }
    }
}
