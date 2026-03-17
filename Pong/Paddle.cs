using Microsoft.Xna.Framework;

namespace Pong;

internal sealed class Paddle
{
    public Paddle(Vector2 position, float width, float height, float speed)
    {
        Position = position;
        Width = width;
        Height = height;
        Speed = speed;
    }

    public Vector2 Position { get; private set; }

    public float Width { get; }

    public float Height { get; }

    public float Speed { get; }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, (int)Width, (int)Height);

    public void Move(float direction, float deltaTime, float screenHeight)
    {
        float nextY = Position.Y + (direction * Speed * deltaTime);
        float clampedY = MathHelper.Clamp(nextY, 0f, screenHeight - Height);
        Position = new Vector2(Position.X, clampedY);
    }

    public void Reset(Vector2 position)
    {
        Position = position;
    }
}
