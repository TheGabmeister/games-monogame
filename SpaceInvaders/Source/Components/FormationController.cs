using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace SpaceInvaders
{
    public class FormationController : Component, IUpdatable
    {
        readonly InvaderData[,] _grid = new InvaderData[Constants.FormationColumns, Constants.FormationRows];
        int _aliveCount = Constants.TotalInvaders;
        int _direction = 1;
        float _fireTimer;
        float _fireInterval;
        int _waveNumber = 1;
        System.Random _rng = new System.Random();
        GameState _gameState;

        public int AliveCount => _aliveCount;

        public void SetInvader(int col, int row, InvaderData data)
        {
            _grid[col, row] = data;
        }

        public void SetWave(int wave)
        {
            _waveNumber = wave;
            _fireInterval = Constants.BaseFireInterval *
                (float)System.Math.Pow(Constants.FireIntervalWaveMultiplier, wave - 1);
            _fireTimer = _fireInterval + (float)_rng.NextDouble() * Constants.FireIntervalVariance;
        }

        public override void OnAddedToEntity()
        {
            _gameState = Entity.Scene.GetSceneComponent<GameState>();
            SetWave(1);
        }

        public void OnInvaderKilled()
        {
            _aliveCount--;
        }

        public void Update()
        {
            if (_gameState.IsGameOver || _aliveCount <= 0)
                return;

            MoveFormation();
            UpdateAnimationSpeed();
            CheckInvasion();
            CheckShieldOverlaps();
            UpdateFiring();
        }

        void MoveFormation()
        {
            float speedMultiplier = (float)Constants.TotalInvaders / MathHelper.Max(_aliveCount, 1);
            float baseSpeed = Constants.BaseFormationSpeed * (float)System.Math.Pow(Constants.WaveSpeedMultiplier, _waveNumber - 1);
            float speed = MathHelper.Min(baseSpeed * speedMultiplier, Constants.MaxFormationSpeed);

            var pos = Entity.Transform.Position;
            pos.X += _direction * speed * Time.DeltaTime;
            Entity.Transform.Position = pos;

            if (ShouldReverse())
            {
                _direction *= -1;
                pos = Entity.Transform.Position;
                pos.Y += Constants.FormationDropDistance;
                Entity.Transform.Position = pos;
            }
        }

        void UpdateAnimationSpeed()
        {
            float speedMultiplier = (float)Constants.TotalInvaders / MathHelper.Max(_aliveCount, 1);
            for (int col = 0; col < Constants.FormationColumns; col++)
            {
                for (int row = 0; row < Constants.FormationRows; row++)
                {
                    var inv = _grid[col, row];
                    if (inv == null || !inv.IsAlive) continue;

                    var animator = inv.Entity.GetComponent<SpriteAnimator>();
                    if (animator != null)
                        animator.Speed = speedMultiplier;
                }
            }
        }

        bool ShouldReverse()
        {
            float formationX = Entity.Transform.Position.X;

            for (int col = 0; col < Constants.FormationColumns; col++)
            {
                for (int row = 0; row < Constants.FormationRows; row++)
                {
                    var inv = _grid[col, row];
                    if (inv == null || !inv.IsAlive) continue;

                    float worldX = formationX + col * Constants.InvaderSpacingX;
                    if (worldX < Constants.FormationMarginX || worldX > Constants.ScreenWidth - Constants.FormationMarginX)
                        return true;
                }
            }
            return false;
        }

        void CheckInvasion()
        {
            float formationY = Entity.Transform.Position.Y;
            for (int row = 0; row < Constants.FormationRows; row++)
            {
                for (int col = 0; col < Constants.FormationColumns; col++)
                {
                    var inv = _grid[col, row];
                    if (inv == null || !inv.IsAlive) continue;

                    float worldY = formationY + row * Constants.InvaderSpacingY;
                    if (worldY >= Constants.InvasionLineY)
                    {
                        _gameState.TriggerGameOver();
                        return;
                    }
                }
            }
        }

        void CheckShieldOverlaps()
        {
            float formationX = Entity.Transform.Position.X;
            float formationY = Entity.Transform.Position.Y;

            for (int col = 0; col < Constants.FormationColumns; col++)
            {
                for (int row = 0; row < Constants.FormationRows; row++)
                {
                    var inv = _grid[col, row];
                    if (inv == null || !inv.IsAlive) continue;

                    float worldX = formationX + col * Constants.InvaderSpacingX;
                    float worldY = formationY + row * Constants.InvaderSpacingY;
                    var invBounds = new RectangleF(worldX - 25, worldY - 20, 50, 40);

                    int shieldMask = 0;
                    Flags.SetFlag(ref shieldMask, PhysicsLayers.Shield);
                    var hits = Physics.BoxcastBroadphase(invBounds, shieldMask);
                    foreach (var hit in hits)
                    {
                        if (hit.Entity != null && !hit.Entity.IsDestroyed)
                            hit.Entity.Destroy();
                    }
                }
            }
        }

        void UpdateFiring()
        {
            _fireTimer -= Time.DeltaTime;
            if (_fireTimer > 0) return;

            _fireTimer = _fireInterval + ((float)_rng.NextDouble() - 0.5f) * Constants.FireIntervalVariance * 2;

            var enemyBullets = Entity.Scene.FindEntitiesWithTag(Tags.EnemyBullet);
            if (enemyBullets.Count >= Constants.MaxEnemyBullets)
                return;

            int col = PickFiringColumn();
            if (col < 0) return;

            var bottomInvader = GetBottomAliveInColumn(col);
            if (bottomInvader == null) return;

            float formationX = Entity.Transform.Position.X;
            float formationY = Entity.Transform.Position.Y;
            var bulletPos = new Vector2(
                formationX + bottomInvader.Column * Constants.InvaderSpacingX,
                formationY + bottomInvader.Row * Constants.InvaderSpacingY + 20
            );

            BulletController.CreateBullet(Entity.Scene, bulletPos, isPlayerBullet: false);
        }

        int PickFiringColumn()
        {
            int attempts = 20;
            while (attempts-- > 0)
            {
                int col = _rng.Next(Constants.FormationColumns);
                if (GetBottomAliveInColumn(col) != null)
                    return col;
            }
            return -1;
        }

        InvaderData GetBottomAliveInColumn(int col)
        {
            for (int row = Constants.FormationRows - 1; row >= 0; row--)
            {
                var inv = _grid[col, row];
                if (inv != null && inv.IsAlive)
                    return inv;
            }
            return null;
        }
    }
}
