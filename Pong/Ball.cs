using Microsoft.Xna.Framework;

namespace Pong;

internal sealed class Ball
{
    public Ball(Vector2 position, float size)
    {
        Position = position;
        Size = size;
        Velocity = Vector2.Zero;
    }

    public Vector2 Position { get; set; }

    public Vector2 Velocity { get; set; }

    public float Size { get; }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, (int)Size, (int)Size);

    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
    }

    public void Reset(Vector2 position)
    {
        Position = position;
        Velocity = Vector2.Zero;
    }
}
