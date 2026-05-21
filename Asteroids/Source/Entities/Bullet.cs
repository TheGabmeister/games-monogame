using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class Bullet
{
    private const float SpriteScale = 8f / 24f;

    private readonly Texture2D _texture;

    public Bullet(Texture2D texture, Vector2 position, Vector2 velocity)
    {
        _texture = texture;
        Position = position;
        Velocity = velocity;
        Lifetime = GameConstants.BulletLifetime;
    }

    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; }
    public float Lifetime { get; private set; }
    public float CollisionRadius => GameConstants.BulletCollisionRadius;
    public bool IsExpired => Lifetime <= 0f;

    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
        Lifetime -= deltaTime;
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var origin = new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f);
        spriteBatch.Draw(_texture, Position, null, Color.White, 0f, origin, SpriteScale, SpriteEffects.None, 0f);
    }
}
