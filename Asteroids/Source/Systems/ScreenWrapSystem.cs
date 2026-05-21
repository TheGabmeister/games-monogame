using Microsoft.Xna.Framework;

public static class ScreenWrapSystem
{
    public static Vector2 Wrap(Vector2 position, float radius)
    {
        return new Vector2(
            WrapAxis(position.X, radius, GameConstants.VirtualWidth),
            WrapAxis(position.Y, radius, GameConstants.VirtualHeight)
        );
    }

    private static float WrapAxis(float value, float radius, float screenSize)
    {
        if (value < -radius)
            return screenSize + radius;
        if (value > screenSize + radius)
            return -radius;
        return value;
    }
}
