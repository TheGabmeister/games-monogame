using Microsoft.Xna.Framework;

namespace Extended
{
    // Shared snapshot of the player's current position, written by PlayerControlSystem
    // and read by systems that need to aim at the player (EmitterSystem). A plain
    // service: there's exactly one player, so this is global state, not a per-entity
    // thing — it lives outside the ECS (see PLAN.md §6).
    public class PlayerTracker
    {
        public Vector2 Position;
        public bool HasPlayer;
    }
}
