using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace SuperMario
{
    public class PlayerController : Component, IUpdatable
    {
        public event Action OnDied;
        const float Gravity = 1200f;
        const float JumpForce = -680f;
        const float MaxFallSpeed = 600f;

        const int SmallWidth = 32;
        const int SmallHeight = 48;
        const int BigWidth = 32;
        const int BigHeight = 64;

        const int MaxFireballs = 2;
        const float InvulnDuration = 2f;

        readonly PrototypeSpriteRenderer _renderer;
        readonly Blinker _blinker;

        VirtualIntegerAxis _moveAxis;
        VirtualButton _jumpButton;
        VirtualButton _fireButton;
        Mover _mover;
        GameState _gameState;
        EntityFactory _factory;
        Vector2 _velocity;
        bool _grounded;
        int _facing = 1;
        int _activeFireballs;
        float _invulnTimer;
        PlayerState _state = PlayerState.Small;

        public PlayerController(PrototypeSpriteRenderer renderer, Blinker blinker)
        {
            _renderer = renderer;
            _blinker = blinker;
        }

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<Mover>();

            _moveAxis = new VirtualIntegerAxis();
            _moveAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right);
            _moveAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D);

            _jumpButton = new VirtualButton();
            _jumpButton.AddKeyboardKey(Keys.Space);
            _jumpButton.AddKeyboardKey(Keys.Up);
            _jumpButton.AddKeyboardKey(Keys.W);

            _fireButton = new VirtualButton();
            _fireButton.AddKeyboardKey(Keys.X);
        }

        public override void OnRemovedFromEntity()
        {
            _moveAxis.Deregister();
            _jumpButton.Deregister();
            _fireButton.Deregister();
        }

        public void Update()
        {
            if (_invulnTimer > 0)
                _invulnTimer -= Time.DeltaTime;

            _velocity.X = _moveAxis.Value * Constants.PlayerSpeed;

            if (_moveAxis.Value != 0)
                _facing = _moveAxis.Value;

            if (_grounded && _jumpButton.IsPressed)
            {
                _velocity.Y = JumpForce;
                Audio.PlaySfx(Assets.Sfx.PlayerJump);
            }

            if (_fireButton.IsPressed && _state == PlayerState.Fire && _activeFireballs < MaxFireballs && _factory != null)
            {
                _factory.CreateFireball(Entity.Scene, Entity.Position, _facing, this);
                _activeFireballs++;
                Audio.PlaySfx(Assets.Sfx.PlayerFire);
            }

            _velocity.Y += Gravity * Time.DeltaTime;
            if (_velocity.Y > MaxFallSpeed)
                _velocity.Y = MaxFallSpeed;

            var movement = _velocity * Time.DeltaTime;
            var result = new CollisionResult();
            _mover.CalculateMovement(ref movement, out result);
            _mover.ApplyMovement(movement);

            _grounded = false;
            if (result.Collider != null && result.Normal.Y < 0)
                _grounded = true;

            if (result.Collider != null)
            {
                if (result.Normal.Y < 0 && _velocity.Y > 0)
                    _velocity.Y = 0;
                if (result.Normal.Y > 0 && _velocity.Y < 0)
                    _velocity.Y = 0;
            }
        }

        public void KillPlayer()
        {
            Audio.PlaySfx(Assets.Sfx.PlayerDie);
            OnDied?.Invoke();
            Entity.Destroy();
        }

        public void TakeHit()
        {
            if (_invulnTimer > 0)
                return;

            if (_state == PlayerState.Small)
            {
                KillPlayer();
                return;
            }

            Audio.PlaySfx(Assets.Sfx.PlayerHit);
            SetState(PlayerState.Small);
            Entity.Position += new Vector2(0, (BigHeight - SmallHeight) / 2f);
            _invulnTimer = InvulnDuration;
            _blinker.Blink(InvulnDuration);
        }

        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
            _state = gameState.PowerState;
            if (_state != PlayerState.Small)
                ApplyStateVisuals();
        }

        public void SetFactory(EntityFactory factory)
        {
            _factory = factory;
        }

        public void NotifyFireballDestroyed()
        {
            if (_activeFireballs > 0)
                _activeFireballs--;
        }

        public void ApplyMushroom()
        {
            if (_state != PlayerState.Small)
                return;

            SetState(PlayerState.Big);
        }

        public void ApplyFireFlower()
        {
            if (_state == PlayerState.Fire)
                return;

            SetState(_state == PlayerState.Small ? PlayerState.Big : PlayerState.Fire);
        }

        void SetState(PlayerState state)
        {
            _state = state;
            if (_gameState != null)
                _gameState.PowerState = _state;
            ApplyStateVisuals();
        }

        void ApplyStateVisuals()
        {
            int w, h;
            Color color;

            switch (_state)
            {
                case PlayerState.Big:
                    w = BigWidth;
                    h = BigHeight;
                    color = Color.Red;
                    break;
                case PlayerState.Fire:
                    w = BigWidth;
                    h = BigHeight;
                    color = Color.OrangeRed;
                    break;
                default:
                    w = SmallWidth;
                    h = SmallHeight;
                    color = Color.Red;
                    break;
            }

            _renderer.SetWidth(w);
            _renderer.SetHeight(h);
            _renderer.SetColor(color);

            var box = Entity.GetComponent<BoxCollider>();
            box.SetSize(w, h);
            box.SetLocalOffset(new Vector2(0, 0));

            if (_state != PlayerState.Small)
                Entity.Position += new Vector2(0, -(BigHeight - SmallHeight) / 2f);
        }
    }
}
