namespace Strikers.Components
{
    public enum PowerUpKind { Weapon, Bomb, Score }

    // A floating pickup that drifts down the screen. The CollisionSystem detects the
    // player touching it and applies the effect by Kind (see PLAN.md §3/§4): raise the
    // weapon level, stock a bomb, or award points.
    public class PowerUp
    {
        public PowerUpKind Kind;

        public PowerUp(PowerUpKind kind)
        {
            Kind = kind;
        }
    }
}
