using Microsoft.Xna.Framework;

namespace Strikers.Components
{
    // Where an entity is in the world. Read by the RenderSystem and written by
    // movement/control systems. Shared by everything visible.
    public class Transform
    {
        public Vector2 Position;
        public float Rotation;
        public float Scale = 1f;

        public Transform(Vector2 position, float rotation = 0f, float scale = 1f)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
