using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class Spiny : Component, IFireballHittable, IStarHittable
    {
        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var spiny = scene.CreateEntity("spiny", center);
            spiny.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.MediumPurple);
            var mover = spiny.AddComponent(new Mover());
            spiny.AddComponent(new GravityBody());

            var body = spiny.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = spiny.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            spiny.AddComponent(new DamagePlayerTrigger());
            spiny.AddComponent(new Spiny());
            spiny.AddComponent(new EnemyWalker(mover, Constants.SpinyWalkSpeed));
        }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }

        public void OnHitByStar(PlayerController player)
        {
            Entity.Destroy();
        }
    }
}
