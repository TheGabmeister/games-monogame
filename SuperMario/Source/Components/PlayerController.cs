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

        VirtualIntegerAxis _moveAxis;
        VirtualButton _jumpButton;
        Mover _mover;
        GameState _gameState;
        Vector2 _velocity;
        bool _grounded;
        PlayerState _state = PlayerState.Small;

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
        }

        public override void OnRemovedFromEntity()
        {
            _moveAxis.Deregister();
            _jumpButton.Deregister();
        }

        public void Update()
        {
            _velocity.X = _moveAxis.Value * Constants.PlayerSpeed;

            if (_grounded && _jumpButton.IsPressed)
            {
                _velocity.Y = JumpForce;
                Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PlayerJump);
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
            Core.GetGlobalManager<SfxManager>().Play(Assets.Sfx.PlayerDie);
            OnDied?.Invoke();
            Entity.Destroy();
        }

        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
            _state = gameState.PowerState;
            if (_state != PlayerState.Small)
                ApplyStateVisuals();
        }

        public void GrowPlayer()
        {
            if (_state == PlayerState.Fire)
                return;

            _state = _state == PlayerState.Small ? PlayerState.Big : PlayerState.Fire;
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

            Entity.RemoveComponent<PrototypeSpriteRenderer>();
            Entity.AddComponent(new PrototypeSpriteRenderer(w, h)).SetColor(color);

            var box = Entity.GetComponent<BoxCollider>();
            box.SetSize(w, h);
            box.SetLocalOffset(new Vector2(0, 0));

            if (_state != PlayerState.Small)
                Entity.Position += new Vector2(0, -(BigHeight - SmallHeight) / 2f);
        }
    }
}
