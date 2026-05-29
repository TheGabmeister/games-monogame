namespace Extended.Components
{
    // Controls when an entity should be despawned. Bullets set DespawnWhenOffscreen
    // so LifetimeSystem destroys them once they leave the playfield. A timed life
    // (for explosions, etc.) can be added here later.
    public class Lifetime
    {
        public bool DespawnWhenOffscreen;

        public Lifetime(bool despawnWhenOffscreen = true)
        {
            DespawnWhenOffscreen = despawnWhenOffscreen;
        }
    }
}
