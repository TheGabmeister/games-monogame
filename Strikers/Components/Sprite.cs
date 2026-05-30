using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Extended.Components
{
    // How an entity is drawn. Size is the desired draw size in virtual pixels;
    // the RenderSystem scales the texture to fit, so a 1x1 placeholder pixel can
    // stand in for any sprite until real art arrives.
    public class Sprite
    {
        public Texture2D Texture;
        public Vector2 Size;
        public Color Color;
        public float LayerDepth;
        public Vector2 Origin;
        // Which part of the texture to draw. null = the whole texture; the
        // AnimationSystem sets this per frame for sprite-sheet animations.
        public Rectangle? SourceRect;

        public Sprite(Texture2D texture, Vector2 size, Color? color = null, float layerDepth = 0f,
                      Rectangle? sourceRect = null)
        {
            Texture = texture;
            Size = size;
            Color = color ?? Color.White;
            LayerDepth = layerDepth;
            SourceRect = sourceRect;
            // Centered origin in the drawn frame's local space: position is the center
            // of the sprite, so rotation and the gameplay hitbox line up at the middle.
            // For an animated sprite the frame size, not the whole sheet, sets the center.
            var frame = sourceRect ?? new Rectangle(0, 0, texture.Width, texture.Height);
            Origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
        }
    }
}
