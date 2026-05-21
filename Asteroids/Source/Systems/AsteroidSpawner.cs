using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class AsteroidSpawner
{
    private readonly Texture2D[] _largeTextures;
    private readonly Texture2D[] _mediumTextures;
    private readonly Texture2D[] _smallTextures;

    public AsteroidSpawner(Texture2D[] largeTextures, Texture2D[] mediumTextures, Texture2D[] smallTextures)
    {
        _largeTextures = largeTextures;
        _mediumTextures = mediumTextures;
        _smallTextures = smallTextures;
    }

    public Asteroid Create(AsteroidSize size, Vector2 position, float waveSpeedMultiplier)
    {
        Texture2D texture = GetTexture(size);
        float speed = Asteroid.GetBaseSpeed(size) * waveSpeedMultiplier * MathHelper.Lerp(0.85f, 1.2f, Random.Shared.NextSingle());
        Vector2 velocity = RandomDirection() * speed;

        return new Asteroid(texture, size, position, velocity);
    }

    public Asteroid CreateChild(AsteroidSize size, Vector2 parentPosition, Vector2 offset, float waveSpeedMultiplier)
    {
        return Create(size, parentPosition + offset, waveSpeedMultiplier);
    }

    public Vector2 GetEdgeSpawnPosition(Vector2 avoidPosition)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            Vector2 position = RandomEdgePosition();

            if (Vector2.DistanceSquared(position, avoidPosition) > GameConstants.SafeRespawnRadius * GameConstants.SafeRespawnRadius)
                return position;
        }

        return RandomEdgePosition();
    }

    private Texture2D GetTexture(AsteroidSize size)
    {
        Texture2D[] textures = size switch
        {
            AsteroidSize.Large => _largeTextures,
            AsteroidSize.Medium => _mediumTextures,
            _ => _smallTextures
        };

        return textures[Random.Shared.Next(textures.Length)];
    }

    private static Vector2 RandomEdgePosition()
    {
        float padding = 50f;
        int edge = Random.Shared.Next(4);

        return edge switch
        {
            0 => new Vector2(Random.Shared.NextSingle() * GameConstants.VirtualWidth, -padding),
            1 => new Vector2(GameConstants.VirtualWidth + padding, Random.Shared.NextSingle() * GameConstants.VirtualHeight),
            2 => new Vector2(Random.Shared.NextSingle() * GameConstants.VirtualWidth, GameConstants.VirtualHeight + padding),
            _ => new Vector2(-padding, Random.Shared.NextSingle() * GameConstants.VirtualHeight)
        };
    }

    private static Vector2 RandomDirection()
    {
        float angle = Random.Shared.NextSingle() * MathHelper.TwoPi;
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }
}
