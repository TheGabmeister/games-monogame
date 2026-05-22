using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Systems;

namespace SpaceInvaders
{
    public class PlayerController : Component, IUpdatable, ITriggerListener
    {
        Entity _activeBullet;
        Collider _collider;
        GameState _gameState;
        SoundEffect _shoot;
        SoundEffect _playerDeath;
        VirtualIntegerAxis _moveInput;
        VirtualButton _fireInput;

        bool _isDead;
        float _deathTimer;
        bool _invulnerable;
        float _invulnerabilityTimer;
        SpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            _renderer = Entity.GetComponent<SpriteRenderer>();
            _gameState = Entity.Scene.GetSceneComponent<GameState>();
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
        }

        public void Update()
        {
            if (_gameState.IsGameOver)
                return;

            if (_isDead)
            {
                _deathTimer -= Time.DeltaTime;
                if (_deathTimer <= 0)
                    Respawn();
                return;
            }

            if (_invulnerable)
            {
                _invulnerabilityTimer -= Time.DeltaTime;
                _renderer.Enabled = ((int)(_invulnerabilityTimer * 10) % 2) == 0;
                if (_invulnerabilityTimer <= 0)
                {
                    _invulnerable = false;
                    _renderer.Enabled = true;
                }
            }

            HandleMovement();
            HandleFiring();
        }

        void HandleMovement()
        {
            int moveDir = _moveInput.Value;
            if (moveDir != 0)
            {
                var pos = Entity.Transform.Position;
                pos.X += moveDir * GameConstants.PlayerSpeed * Time.DeltaTime;
                pos.X = MathHelper.Clamp(pos.X, GameConstants.PlayerMarginX, GameConstants.ScreenWidth - GameConstants.PlayerMarginX);
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
            _deathTimer = GameConstants.DeathDelay;
            _renderer.Enabled = false;
            _playerDeath.Play();
            if (_collider != null)
                _collider.Enabled = false;
            _gameState.LoseLife();
        }

        void Respawn()
        {
            _isDead = false;
            _renderer.Enabled = true;
            if (_collider != null)
                _collider.Enabled = true;
            Entity.Transform.Position = new Vector2(GameConstants.ScreenWidth / 2f, GameConstants.PlayerY);
            _invulnerable = true;
            _invulnerabilityTimer = GameConstants.RespawnInvulnerability;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == (1 << PhysicsLayers.EnemyBullet))
                Die();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
