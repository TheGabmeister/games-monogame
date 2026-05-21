using Microsoft.Xna.Framework;

public static class ScreenWrapSystem
{
    public static Vector2 Wrap(Vector2 position, float radius)
    {
        if (position.X < -radius)
            position.X = GameConstants.VirtualWidth + radius;
        else if (position.X > GameConstants.VirtualWidth + radius)
            position.X = -radius;

        if (position.Y < -radius)
            position.Y = GameConstants.VirtualHeight + radius;
        else if (position.Y > GameConstants.VirtualHeight + radius)
            position.Y = -radius;

        return position;
    }
}
