namespace Extended.Components
{
    // Controls when an entity should be despawned. Bullets set DespawnWhenOffscreen so
    // LifetimeSystem destroys them once they leave the playfield; short-lived effects
    // (explosions) use a countdown timer instead.
    public class Lifetime
    {
        public bool DespawnWhenOffscreen;
        // When HasTimer is set, SecondsRemaining counts down and the entity is destroyed
        // at zero. Used by fixed-duration effects like explosions.
        public bool HasTimer;
        public float SecondsRemaining;

        public Lifetime(bool despawnWhenOffscreen = true)
        {
            DespawnWhenOffscreen = despawnWhenOffscreen;
        }

        // A fixed-duration life (e.g. an explosion that lasts exactly its animation).
        public static Lifetime Timer(float seconds) =>
            new Lifetime(despawnWhenOffscreen: false) { HasTimer = true, SecondsRemaining = seconds };
    }
}
