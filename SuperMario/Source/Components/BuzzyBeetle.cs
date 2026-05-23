using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class BuzzyBeetle : Component, IFireballHittable, IStompable, IStarHittable
    {
        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var buzzy = scene.CreateEntity("buzzybeetle", center);
            buzzy.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.DarkSlateBlue);
            var mover = buzzy.AddComponent(new Mover());
            buzzy.AddComponent(new GravityBody());

            var body = buzzy.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = buzzy.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            buzzy.AddComponent(new DamagePlayerTrigger());
            buzzy.AddComponent(new BuzzyBeetle());
            buzzy.AddComponent(new EnemyWalker(mover, Constants.BuzzyBeetleWalkSpeed));
        }

        public void OnStomped(PlayerController player)
        {
            Entity.Destroy();
        }

        public FireballReaction OnHitByFireball()
        {
            return FireballReaction.Blocked;
        }

        public void OnHitByStar(PlayerController player)
        {
            Entity.Destroy();
        }
    }
}
