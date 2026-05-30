using Microsoft.Xna.Framework;

namespace Strikers.Components
{
    // Tags the player-controlled entity and holds its control state plus its arcade
    // stats. InputSystem writes the intent (MoveDirection/Firing/BombPressed);
    // PlayerControlSystem, WeaponSystem and BombSystem consume it. Lives/bombs/weapon
    // level are this entity's gameplay data, so they live here, not in GameState (which
    // owns only global flow + score — see PLAN.md §3/§6).
    public class Player
    {
        public float Speed;            // virtual pixels per second
        public Vector2 MoveDirection;  // normalized intent from InputSystem
        public bool Firing;            // fire-button intent from InputSystem
        public bool BombPressed;       // edge-triggered bomb intent from InputSystem
        public float InvulnTimer;      // seconds of invulnerability remaining (spawn/respawn i-frames)

        public int Lives;              // spare ships in reserve; game over when the last one dies
        public int Bombs;              // bomb stock
        public int WeaponLevel;        // main-shot power level, raised by weapon power-ups

        public Player(float speed)
        {
            Speed = speed;
            WeaponLevel = 1;
        }
    }
}
