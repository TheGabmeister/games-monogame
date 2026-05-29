namespace Extended.Components
{
    // A circular gameplay hitbox centered on the entity's Transform.Position. Layer
    // is what this entity is; Mask is what it collides with. The CollisionSystem only
    // tests a pair when their layers mask each other (see PLAN.md §4).
    public class CircleCollider
    {
        public float Radius;
        public CollisionLayer Layer;
        public CollisionLayer Mask;

        public CircleCollider(float radius, CollisionLayer layer, CollisionLayer mask = CollisionLayer.None)
        {
            Radius = radius;
            Layer = layer;
            Mask = mask;
        }
    }
}
