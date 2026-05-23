using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class Blooper : Component, IUpdatable, IFireballHittable, IStarHittable
    {
        readonly Mover _mover;
        float _bobTimer;
        PlayerController _player;

        public Blooper(Mover mover)
        {
            _mover = mover;
        }

        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var blooper = scene.CreateEntity("blooper", center);
            blooper.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.White);
            var mover = blooper.AddComponent(new Mover());

            var body = blooper.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = blooper.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            blooper.AddComponent(new DamagePlayerTrigger());
            blooper.AddComponent(new Blooper(mover));
        }

        public void Update()
        {
            if (_player == null || _player.Entity == null || _player.Entity.IsDestroyed)
                _player = Entity.Scene?.FindComponentOfType<PlayerController>();

            var motion = Vector2.Zero;
            if (_player != null)
            {
                var toPlayer = _player.Entity.Position - Entity.Position;
                if (toPlayer.LengthSquared() > 1f)
                    motion = Vector2.Normalize(toPlayer) * Constants.BlooperSpeed;
            }

            _bobTimer += Time.DeltaTime * Constants.BlooperBobSpeed;
            motion.Y += Mathf.Sin(_bobTimer) * Constants.BlooperBobStrength;

            motion *= Time.DeltaTime;
            _mover.CalculateMovement(ref motion, out _);
            _mover.ApplyMovement(motion);
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
