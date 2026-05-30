using Microsoft.Xna.Framework;

namespace Strikers.Components
{
    // Velocity in virtual pixels per second. The MovementSystem integrates this
    // into Transform.Position each frame. Used by bullets/enemies (Phase 1+).
    public class Velocity
    {
        public Vector2 Value;

        public Velocity(Vector2 value = default)
        {
            Value = value;
        }
    }
}
