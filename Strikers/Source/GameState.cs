namespace Strikers
{
    // The arcade loop's flow state. Which screen we're on (playing the stage, the
    // STAGE CLEAR banner, or GAME OVER) plus the global score. This is singleton flow
    // control, not per-entity data, so it's a plain service that systems read/write
    // rather than ECS state (see PLAN.md §6). Lives/bombs/weapon level stay on the
    // Player component because they're that entity's gameplay data.
    public enum GamePhase { Playing, StageClear, GameOver }

    public class GameState
    {
        public GamePhase Phase = GamePhase.Playing;
        public int Score;
    }
}
