using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class Goomba : Component, IFireballHittable, IStompable
    {
        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var goomba = scene.CreateEntity("goomba", center);
            goomba.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Brown);
            var mover = goomba.AddComponent(new Mover());
            goomba.AddComponent(new GravityBody());

            var body = goomba.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = goomba.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            goomba.AddComponent(new DamagePlayerTrigger());
            goomba.AddComponent(new Goomba());
            goomba.AddComponent(new EnemyWalker(mover, Constants.GoombaWalkSpeed));
        }

        public void OnStomped(PlayerController player)
        {
            Entity.Destroy();
        }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }
    }
}
