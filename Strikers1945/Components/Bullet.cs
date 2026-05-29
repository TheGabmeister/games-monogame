namespace Extended.Components
{
    // Tags a projectile and carries how much damage it deals on hit. CollisionSystem
    // reads Damage in Phase 2; for now it just marks bullets so they can be told
    // apart from other moving entities.
    public class Bullet
    {
        public int Damage;

        public Bullet(int damage)
        {
            Damage = damage;
        }
    }
}
