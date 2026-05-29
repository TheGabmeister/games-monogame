using Extended.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Extended.Systems
{
    // Draws every entity that has a Transform + Sprite. The texture is scaled to
    // the sprite's requested Size, so 1x1 placeholder pixels render as quads of
    // any size and real sprites drop in unchanged later.
    public class RenderSystem : EntityDrawSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private ComponentMapper<Transform> _transformMapper;
        private ComponentMapper<Sprite> _spriteMapper;

        public RenderSystem(SpriteBatch spriteBatch)
            : base(Aspect.All(typeof(Transform), typeof(Sprite)))
        {
            _spriteBatch = spriteBatch;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform>();
            _spriteMapper = mapperService.GetMapper<Sprite>();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.BackToFront, samplerState: SamplerState.PointClamp);

            foreach (var entityId in ActiveEntities)
            {
                var transform = _transformMapper.Get(entityId);
                var sprite = _spriteMapper.Get(entityId);

                var frameSize = sprite.SourceRect.HasValue
                    ? new Vector2(sprite.SourceRect.Value.Width, sprite.SourceRect.Value.Height)
                    : new Vector2(sprite.Texture.Width, sprite.Texture.Height);
                var scale = sprite.Size / frameSize * transform.Scale;

                _spriteBatch.Draw(
                    sprite.Texture,
                    transform.Position,
                    sprite.SourceRect,
                    sprite.Color,
                    transform.Rotation,
                    sprite.Origin,
                    scale,
                    SpriteEffects.None,
                    sprite.LayerDepth);
            }

            _spriteBatch.End();
        }
    }
}
