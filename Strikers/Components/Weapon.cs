namespace Strikers.Components
{
    // An auto-firing main shot. WeaponSystem counts Cooldown down each frame and,
    // while the owner is firing, spawns a bullet every FireInterval seconds.
    // Pattern id / level get added when Phase 3/4 introduce power-ups.
    public class Weapon
    {
        public float FireInterval;  // seconds between shots
        public float Cooldown;      // counts down to 0, then a shot is allowed
        public float BulletSpeed;   // virtual pixels per second
        public int Damage;

        public Weapon(float fireInterval, float bulletSpeed, int damage)
        {
            FireInterval = fireInterval;
            BulletSpeed = bulletSpeed;
            Damage = damage;
        }
    }
}
