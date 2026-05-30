using System;

namespace Extended.Components
{
    // Bitmask of collision categories (see PLAN.md §4). A collider's Layer says what
    // it is; its Mask says which layers it collides with. [Flags] from the start so a
    // new category (boss, terrain, ...) is just another bit — no refactor.
    [Flags]
    public enum CollisionLayer
    {
        None         = 0,
        Player       = 1 << 0,
        PlayerBullet = 1 << 1,
        Enemy        = 1 << 2,
        EnemyBullet  = 1 << 3,
        PowerUp      = 1 << 4,
    }
}
