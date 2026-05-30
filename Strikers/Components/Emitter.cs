using Microsoft.Xna.Framework;

namespace Extended.Components
{
    public enum BulletPattern { Aimed, Spread, Ring, Spiral }
    public enum BulletKind { Round, Needle }

    // Data-driven enemy bullet emitter (see PLAN.md §4). The EmitterSystem reads these
    // numbers to spawn a volley every FireInterval seconds — patterns are authored by
    // tuning values, not by writing per-enemy code. Angles are radians in screen space
    // (Y points down), so "down the screen" is +PiOver2.
    public class Emitter
    {
        public BulletPattern Pattern;
        public BulletKind Kind;
        public float FireInterval;  // seconds between volleys
        public float Cooldown;      // counts down to 0, then a volley fires
        public float BulletSpeed;   // virtual px/sec
        public int Damage;
        public int BulletCount;     // bullets per volley (Spread/Ring/Spiral)
        public float SpreadAngle;   // total arc of a Spread/Aimed volley (radians)
        public float BaseAngle;     // base direction for Spread/Ring (radians; default down)
        public float SpinRate;      // radians the Spiral base angle advances each volley
        public float SpinAngle;     // accumulated Spiral angle (runtime state)

        public Emitter(BulletPattern pattern, BulletKind kind, float fireInterval, float bulletSpeed,
                       int damage, int bulletCount = 1, float spreadAngle = 0f,
                       float baseAngle = MathHelper.PiOver2, float spinRate = 0f)
        {
            Pattern = pattern;
            Kind = kind;
            FireInterval = fireInterval;
            BulletSpeed = bulletSpeed;
            Damage = damage;
            BulletCount = bulletCount;
            SpreadAngle = spreadAngle;
            BaseAngle = baseAngle;
            SpinRate = spinRate;
        }
    }
}
