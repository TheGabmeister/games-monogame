using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class Texture2DExtensions
{
    public static Vector2 Center(this Texture2D texture)
    {
        return new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
    }
}
