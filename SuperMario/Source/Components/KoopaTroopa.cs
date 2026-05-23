using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class KoopaTroopa : Component, IFireballHittable, IStompable, IStarHittable
    {
        public KoopaTroopa(KoopaColor color)
        {
            Color = color;
        }

        public KoopaColor Color { get; }

        public static void SpawnGreen(Scene scene, TmxObject obj)
        {
            Spawn(scene, EntityFactory.GetCenter(obj), obj.Width, obj.Height, KoopaColor.Green);
        }

        public static void SpawnRed(Scene scene, TmxObject obj)
        {
            Spawn(scene, EntityFactory.GetCenter(obj), obj.Width, obj.Height, KoopaColor.Red);
        }

        public static Entity Spawn(Scene scene, Vector2 position, float width, float height, KoopaColor color)
        {
            var isRed = color == KoopaColor.Red;
            var koopa = scene.CreateEntity(isRed ? "redkoopatroopa" : "greenkoopatroopa", position);
            koopa.AddComponent(new PrototypeSpriteRenderer(width, height)).SetColor(isRed ? Microsoft.Xna.Framework.Color.Red : Microsoft.Xna.Framework.Color.Green);
            var mover = koopa.AddComponent(new Mover());
            koopa.AddComponent(new GravityBody());

            var body = koopa.AddComponent(new BoxCollider(-width / 2f, -height / 2f, width, height));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = koopa.AddComponent(new BoxCollider(-width / 2f, -height / 2f, width, height));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            koopa.AddComponent(new DamagePlayerTrigger());
            koopa.AddComponent(new KoopaTroopa(color));
            koopa.AddComponent(new EnemyWalker(
                mover,
                isRed ? Constants.RedKoopaTroopaWalkSpeed : Constants.GreenKoopaTroopaWalkSpeed));
            return koopa;
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

        public void OnHitByStar(PlayerController player)
        {
            Entity.Destroy();
        }
    }
}
