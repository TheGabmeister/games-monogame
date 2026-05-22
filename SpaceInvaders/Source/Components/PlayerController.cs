using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class PlayerController : Component, IUpdatable, ITriggerListener
    {
        public event Action Died;

        Entity _activeBullet;
        SoundEffect _shoot;
        SoundEffect _playerDeath;
        VirtualIntegerAxis _moveInput;
        VirtualButton _fireInput;
        Blinker _blinker;

        bool _isDead;
        bool _startInvulnerable;
        bool _invulnerable;
        float _invulnerabilityTimer;

        public PlayerController() : this(false)
        {
        }

        public PlayerController(bool startInvulnerable)
        {
            _startInvulnerable = startInvulnerable;
        }

        public override void OnAddedToEntity()
        {
            _blinker = Entity.GetComponent<Blinker>();
            _shoot = Entity.Scene.Content.LoadSoundEffect(Assets.Audio.Sfx.Shoot);
            _playerDeath = Entity.Scene.Content.LoadSoundEffect(Assets.Audio.Sfx.PlayerDeath);

            _moveInput = new VirtualIntegerAxis();
            _moveInput.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right);
            _moveInput.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D);
            _moveInput.AddGamePadLeftStickX();
            _moveInput.AddGamePadDPadLeftRight();

            _fireInput = new VirtualButton();
            _fireInput.AddKeyboardKey(Keys.Space);
            _fireInput.AddGamePadButton(0, Buttons.A);

            if (_startInvulnerable)
                StartInvulnerability();
        }

        public override void OnRemovedFromEntity()
        {
            _moveInput?.Deregister();
            _fireInput?.Deregister();
            Died = null;
        }

        public void Update()
        {
            if (_isDead)
                return;

            UpdateInvulnerability();
            HandleMovement();
            HandleFiring();
        }

        void UpdateInvulnerability()
        {
            if (!_invulnerable)
                return;

            _invulnerabilityTimer -= Time.DeltaTime;
            if (_invulnerabilityTimer > 0)
                return;

            _invulnerable = false;
            if (_blinker != null)
                _blinker.Enabled = false;
        }

        void HandleMovement()
        {
            int moveDir = _moveInput.Value;
            if (moveDir != 0)
            {
                var pos = Entity.Transform.Position;
                pos.X += moveDir * Constants.PlayerSpeed * Time.DeltaTime;
                pos.X = MathHelper.Clamp(pos.X, Constants.PlayerMarginX, Constants.ScreenWidth - Constants.PlayerMarginX);
                Entity.Transform.Position = pos;
            }
        }

        void HandleFiring()
        {
            if (_activeBullet != null && !_activeBullet.IsDestroyed)
                return;

            if (_fireInput.IsDown)
            {
                var bulletPos = Entity.Transform.Position + new Vector2(0, -20);
                _activeBullet = BulletController.CreateBullet(Entity.Scene, bulletPos, isPlayerBullet: true);
                _shoot.Play();
            }
        }

        public void Die()
        {
            if (_isDead || _invulnerable)
                return;

            _isDead = true;
            _playerDeath.Play();
            Died?.Invoke();
            Entity.Destroy();
        }

        void StartInvulnerability()
        {
            _invulnerable = true;
            _invulnerabilityTimer = Constants.RespawnInvulnerability;
            if (_blinker != null)
                _blinker.Enabled = true;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == (1 << PhysicsLayers.EnemyBullet))
            {
                if (!_invulnerable)
                    Die();
            }  
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
