using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class PlayerShip
{
    private const float RotationSpeed = 4.8f;
    private const float ThrustAcceleration = 370f;
    private const float MaxSpeed = 320f;
    private const float SpriteScale = 42f / 64f;

    private readonly Texture2D _texture;
    private float _fireTimer;

    public PlayerShip(Texture2D texture)
    {
        _texture = texture;
        ResetToCenter();
    }

    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }
    public float Rotation { get; private set; }
    public float InvulnerabilityTimer { get; private set; }
    public bool IsAlive { get; private set; } = true;
    public bool IsThrusting { get; private set; }
    public bool CanFire => IsAlive && _fireTimer <= 0f;
    public float CollisionRadius => GameConstants.ShipCollisionRadius;
    public bool IsInvulnerable => InvulnerabilityTimer > 0f;

    public Vector2 Forward => new Vector2(MathF.Sin(Rotation), -MathF.Cos(Rotation));

    public void ResetToCenter()
    {
        Position = new Vector2(GameConstants.VirtualWidth * 0.5f, GameConstants.VirtualHeight * 0.5f);
        Velocity = Vector2.Zero;
        Rotation = 0f;
        InvulnerabilityTimer = GameConstants.RespawnInvulnerability;
        IsAlive = true;
        IsThrusting = false;
        _fireTimer = 0f;
    }

    public void Kill()
    {
        IsAlive = false;
        IsThrusting = false;
        Velocity = Vector2.Zero;
    }

    public void MarkFired()
    {
        _fireTimer = GameConstants.FireCooldown;
    }

    public void Update(float deltaTime, InputState input)
    {
        if (_fireTimer > 0f)
            _fireTimer -= deltaTime;

        if (InvulnerabilityTimer > 0f)
            InvulnerabilityTimer -= deltaTime;

        if (!IsAlive)
        {
            IsThrusting = false;
            return;
        }

        Rotation += input.RotationAxis * RotationSpeed * deltaTime;
        IsThrusting = input.IsThrustDown;

        if (IsThrusting)
            Velocity += Forward * ThrustAcceleration * deltaTime;

        float speed = Velocity.Length();

        if (speed > MaxSpeed)
            Velocity = Velocity / speed * MaxSpeed;

        Position += Velocity * deltaTime;
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive)
            return;

        if (IsInvulnerable && ((int)(InvulnerabilityTimer * 12f) % 2 == 0))
            return;

        var origin = new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f);
        spriteBatch.Draw(_texture, Position, null, Color.White, Rotation, origin, SpriteScale, SpriteEffects.None, 0f);
    }
}
