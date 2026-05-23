using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class KoopaParatroopa : Component, IUpdatable, IFireballHittable, IStompable, IStarHittable
    {
        readonly Mover _mover;
        readonly KoopaColor _color;
        readonly float _width;
        readonly float _height;
        readonly float _flySpeed;
        int _direction = -1;

        public KoopaParatroopa(
            Mover mover,
            KoopaColor color,
            float width,
            float height,
            float flySpeed)
        {
            _mover = mover;
            _color = color;
            _width = width;
            _height = height;
            _flySpeed = flySpeed;
        }

        public static void SpawnGreen(Scene scene, TmxObject obj)
        {
            Spawn(scene, obj, KoopaColor.Green);
        }

        public static void SpawnRed(Scene scene, TmxObject obj)
        {
            Spawn(scene, obj, KoopaColor.Red);
        }

        static void Spawn(Scene scene, TmxObject obj, KoopaColor color)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;
            var isRed = color == KoopaColor.Red;

            var para = scene.CreateEntity(isRed ? "redkoopaparatroopa" : "greenkoopaparatroopa", center);
            para.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(isRed ? Color.IndianRed : Color.LightGreen);
            var mover = para.AddComponent(new Mover());

            var body = para.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = para.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            para.AddComponent(new DamagePlayerTrigger());
            para.AddComponent(new KoopaParatroopa(
                mover,
                color,
                w,
                h,
                isRed ? Constants.RedKoopaParatroopaFlySpeed : Constants.GreenKoopaParatroopaFlySpeed));
        }

        public void Update()
        {
            var motion = new Vector2(_direction * _flySpeed * Time.DeltaTime, 0);
            _mover.CalculateMovement(ref motion, out var result);
            _mover.ApplyMovement(motion);

            if (result.Collider != null && result.Normal.X != 0)
                _direction *= -1;
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
