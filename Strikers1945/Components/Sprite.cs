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

        public Sprite(Texture2D texture, Vector2 size, Color? color = null, float layerDepth = 0f)
        {
            Texture = texture;
            Size = size;
            Color = color ?? Color.White;
            LayerDepth = layerDepth;
            // Centered origin: position is the center of the sprite, which also
            // makes rotation and the gameplay hitbox line up at the middle.
            Origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        }
    }
}
