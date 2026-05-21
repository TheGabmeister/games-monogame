using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids.Entities
{
    public sealed class Asteroid
    {
        private readonly Texture2D _texture;
        private readonly float _spriteScale;

        public Asteroid(Texture2D texture, AsteroidSize size, Vector2 position, Vector2 velocity)
        {
            _texture = texture;
            Size = size;
            Position = position;
            Velocity = velocity;
            Radius = GetRadius(size);
            _spriteScale = GetTargetDiameter(size) / texture.Width;
            Rotation = Random.Shared.NextSingle() * MathHelper.TwoPi;
            RotationSpeed = MathHelper.Lerp(-1.6f, 1.6f, Random.Shared.NextSingle());
        }

        public AsteroidSize Size { get; }
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; }
        public float Radius { get; }
        public float Rotation { get; private set; }
        public float RotationSpeed { get; }

        public int ScoreValue => Size switch
        {
            AsteroidSize.Large => 20,
            AsteroidSize.Medium => 50,
            _ => 100
        };

        public void Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
            Rotation += RotationSpeed * deltaTime;
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var origin = new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f);
            spriteBatch.Draw(_texture, Position, null, Color.White, Rotation, origin, _spriteScale, SpriteEffects.None, 0f);
        }

        public static float GetRadius(AsteroidSize size) => size switch
        {
            AsteroidSize.Large => 42f,
            AsteroidSize.Medium => 25f,
            _ => 14f
        };

        public static float GetBaseSpeed(AsteroidSize size) => size switch
        {
            AsteroidSize.Large => 70f,
            AsteroidSize.Medium => 105f,
            _ => 145f
        };

        private static float GetTargetDiameter(AsteroidSize size) => size switch
        {
            AsteroidSize.Large => 88f,
            AsteroidSize.Medium => 52f,
            _ => 30f
        };
    }
}
