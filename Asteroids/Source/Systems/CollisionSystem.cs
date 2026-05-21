using Microsoft.Xna.Framework;

public static class CollisionSystem
{
    public static bool CirclesOverlap(Vector2 aPosition, float aRadius, Vector2 bPosition, float bRadius)
    {
        float combinedRadius = aRadius + bRadius;
        return Vector2.DistanceSquared(aPosition, bPosition) <= combinedRadius * combinedRadius;
    }
}
