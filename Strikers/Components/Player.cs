using Microsoft.Xna.Framework;

namespace Strikers.Components
{
    // Tags the player-controlled entity and holds its control state. InputSystem
    // writes the intent (MoveDirection); PlayerControlSystem consumes it.
    // Weapon level, bombs and lives get added in later phases.
    public class Player
    {
        public float Speed;            // virtual pixels per second
        public Vector2 MoveDirection;  // normalized intent from InputSystem
        public bool Firing;            // fire-button intent from InputSystem
        public float InvulnTimer;      // seconds of invulnerability remaining (spawn/respawn i-frames)

        public Player(float speed)
        {
            Speed = speed;
        }
    }
}
