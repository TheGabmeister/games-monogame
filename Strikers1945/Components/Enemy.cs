namespace Extended.Components
{
    public enum EnemyType { Popcorn, Fighter, Gunship }

    // Tags an enemy and carries its archetype + score award. Movement-pattern / path
    // data joins this in Phase 3 (see PLAN.md §3).
    public class Enemy
    {
        public EnemyType Type;
        public int ScoreValue;

        public Enemy(EnemyType type, int scoreValue)
        {
            Type = type;
            ScoreValue = scoreValue;
        }
    }
}
