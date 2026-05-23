using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class Starman : Component, IUpdatable, ITriggerListener
    {
        readonly GameState _gameState;
        GravityBody _body;
        int _direction = 1;
        bool _collected;

        public Starman(GameState gameState)
        {
            _gameState = gameState;
            UpdateOrder = 10;
        }

        public static void Spawn(Scene scene, TmxObject obj, GameState gameState)
        {
            var center = EntityFactory.GetCenter(obj);
            var w = obj.Width;
            var h = obj.Height;

            var starman = scene.CreateEntity("starman", center);
            starman.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(Color.Yellow);
            starman.AddComponent(new Mover());
            starman.AddComponent(new GravityBody());

            var body = starman.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            body.PhysicsLayer = 1 << PhysicsLayers.PickupBody;
            body.CollidesWithLayers = 1 << PhysicsLayers.Environment;

            var pickup = starman.AddComponent(new BoxCollider(-w / 2f, -h / 2f, w, h));
            pickup.IsTrigger = true;
            pickup.PhysicsLayer = 1 << PhysicsLayers.PickupTrigger;
            pickup.CollidesWithLayers = 1 << PhysicsLayers.Player;

            starman.AddComponent(new Starman(gameState));
        }

        public override void OnAddedToEntity()
        {
            _body = Entity.GetComponent<GravityBody>();
            _body.Velocity.X = _direction * Constants.StarmanSpeed;
        }

        public void Update()
        {
            if (_body.LastCollision.Collider != null && _body.LastCollision.Normal.X != 0)
                _direction *= -1;

            _body.Velocity.X = _direction * Constants.StarmanSpeed;

            if (_body.IsGrounded)
                _body.Velocity.Y = Constants.StarmanBounceForce;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (_collected)
                return;

            var player = other.Entity.GetComponent<PlayerController>();
            if (player == null)
                return;

            _collected = true;
            _gameState.AddScore(Constants.StarmanPickupScore);
            ScorePopup.Spawn(Entity.Scene, Entity.Position, Constants.StarmanPickupScore);
            Audio.PlaySfx(Assets.Sfx.PickupMushroom);
            player.ApplyStarman();
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
