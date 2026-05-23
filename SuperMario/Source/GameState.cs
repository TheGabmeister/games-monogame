namespace SuperMario
{
    public class GameState
    {
        public int Score { get; set; }
        public int Lives { get; set; } = Constants.StartingLives;
        public PlayerState PowerState { get; set; } = PlayerState.Small;
    }
}
