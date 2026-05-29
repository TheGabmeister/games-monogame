using Microsoft.Xna.Framework;

namespace Extended.Components
{
    // Tags the player-controlled entity and holds its control state. InputSystem
    // writes the intent (MoveDirection); PlayerControlSystem consumes it.
    // Weapon level, bombs, lives and invuln timer get added in later phases.
    public class Player
    {
        public float Speed;            // virtual pixels per second
        public Vector2 MoveDirection;  // normalized intent from InputSystem
        public bool Firing;            // fire-button intent from InputSystem

        public Player(float speed)
        {
            Speed = speed;
        }
    }
}
