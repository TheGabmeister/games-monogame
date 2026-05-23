using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Tiled;

namespace SuperMario
{
    public class HammerBro : Component, IUpdatable, IFireballHittable, IStompable, IStarHittable
    {
        readonly SpriteRenderer _renderer;
        GravityBody _body;
        PlayerController _player;
        int _facing = -1;
        float _shuffleTimer;
        int _shuffleDir = -1;
        float _jumpTimer;
        float _throwTimer;

        public HammerBro(SpriteRenderer renderer)
        {
            _renderer = renderer;
        }

        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var bro = scene.CreateEntity("hammerbro", center);
            var renderer = bro.AddComponent(new PrototypeSpriteRenderer(w, h));
            renderer.SetColor(Color.DarkOliveGreen);
            bro.AddComponent(new Mover());
            bro.AddComponent(new GravityBody());

            var body = bro.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var hit = bro.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            hit.IsTrigger = true;
            hit.PhysicsLayer = 1 << PhysicsLayers.Enemy;
            hit.CollidesWithLayers = 1 << PhysicsLayers.Player;

            bro.AddComponent(new DamagePlayerTrigger());
            bro.AddComponent(new HammerBro(renderer));
        }

        public override void OnAddedToEntity()
        {
            _body = Entity.GetComponent<GravityBody>();
            _throwTimer = Constants.HammerBroThrowInterval * 0.5f;
            _jumpTimer = Constants.HammerBroJumpInterval * 0.5f;
        }

        public void Update()
        {
            if (_player == null || _player.Entity == null || _player.Entity.IsDestroyed)
                _player = Entity.Scene?.FindComponentOfType<PlayerController>();

            if (_player != null)
            {
                _facing = _player.Entity.Position.X < Entity.Position.X ? -1 : 1;
                if (_renderer != null)
                    _renderer.FlipX = _facing < 0;
            }

            _shuffleTimer += Time.DeltaTime;
            if (_shuffleTimer >= Constants.HammerBroShuffleInterval)
            {
                _shuffleTimer = 0f;
                _shuffleDir *= -1;
            }
            _body.Velocity.X = _shuffleDir * Constants.HammerBroShuffleSpeed;

            _jumpTimer += Time.DeltaTime;
            if (_jumpTimer >= Constants.HammerBroJumpInterval)
            {
                _jumpTimer = 0f;
                if (_body.IsGrounded)
                    _body.Velocity.Y = Constants.HammerBroJumpForce;
            }

            _throwTimer += Time.DeltaTime;
            if (_throwTimer >= Constants.HammerBroThrowInterval)
            {
                _throwTimer = 0f;
                Hammer.Spawn(Entity.Scene, Entity.Position, _facing);
            }
        }

        public FireballReaction OnHitByFireball()
        {
            Entity.Destroy();
            return FireballReaction.Defeated;
        }

        public void OnStomped(PlayerController player)
        {
            Entity.Destroy();
        }

        public void OnHitByStar(PlayerController player)
        {
            Entity.Destroy();
        }
    }
}
