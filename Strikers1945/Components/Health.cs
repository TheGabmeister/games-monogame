namespace Extended.Components
{
    // Hit points. CollisionSystem subtracts from Current; DamageSystem reaps the
    // entity once it reaches zero (see PLAN.md §4).
    public class Health
    {
        public int Current;
        public int Max;

        public Health(int max)
        {
            Max = max;
            Current = max;
        }
    }
}
