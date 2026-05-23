using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class Podoboo : Component, IUpdatable, IFireballHittable
    {
        Mover _mover;
        Vector2 _homePosition;
        Vector2 _velocity;
        float _restTimer;
        bool _resting;

        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var podoboo = scene.CreateEntity("podoboo", center);
            podoboo.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.OrangeRed);
            podoboo.AddComponent(new Mover());

            var hit = podoboo.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            podoboo.AddComponent(new DamagePlayerTrigger());
            podoboo.AddComponent(new Podoboo());
        }

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();
            _homePosition = Entity.Position;
            Launch();
        }

        public void Update()
        {
            if (_resting)
            {
                _restTimer -= Time.DeltaTime;
                if (_restTimer <= 0f)
                    Launch();
                return;
            }

            _velocity.Y += Constants.PodobooGravity * Time.DeltaTime;
            var motion = _velocity * Time.DeltaTime;
            _mover.ApplyMovement(motion);

            if (_velocity.Y > 0f && Entity.Position.Y >= _homePosition.Y)
            {
                Entity.Position = _homePosition;
                _velocity = Vector2.Zero;
                _resting = true;
                _restTimer = Constants.PodobooRestDuration;
            }
        }

        void Launch()
        {
            _resting = false;
            _velocity = new Vector2(0f, -Constants.PodobooJumpSpeed);
        }

        public FireballReaction OnHitByFireball()
        {
            return FireballReaction.Blocked;
        }
    }
}
