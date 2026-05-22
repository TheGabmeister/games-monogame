using Microsoft.Xna.Framework;
using Nez;

namespace SpaceInvaders
{
    public class FormationController : Component, IUpdatable
    {
        readonly InvaderData[,] _grid = new InvaderData[GameConstants.FormationColumns, GameConstants.FormationRows];
        int _aliveCount = GameConstants.TotalInvaders;
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
            _fireInterval = GameConstants.BaseFireInterval *
                (float)System.Math.Pow(GameConstants.FireIntervalWaveMultiplier, wave - 1);
            _fireTimer = _fireInterval + (float)_rng.NextDouble() * GameConstants.FireIntervalVariance;
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
            CheckInvasion();
            CheckShieldOverlaps();
            UpdateFiring();
        }

        void MoveFormation()
        {
            float speedMultiplier = (float)GameConstants.TotalInvaders / MathHelper.Max(_aliveCount, 1);
            float baseSpeed = GameConstants.BaseFormationSpeed * (float)System.Math.Pow(GameConstants.WaveSpeedMultiplier, _waveNumber - 1);
            float speed = MathHelper.Min(baseSpeed * speedMultiplier, GameConstants.MaxFormationSpeed);

            var pos = Entity.Transform.Position;
            pos.X += _direction * speed * Time.DeltaTime;
            Entity.Transform.Position = pos;

            if (ShouldReverse())
            {
                _direction *= -1;
                pos = Entity.Transform.Position;
                pos.Y += GameConstants.FormationDropDistance;
                Entity.Transform.Position = pos;
            }
        }

        bool ShouldReverse()
        {
            float formationX = Entity.Transform.Position.X;

            for (int col = 0; col < GameConstants.FormationColumns; col++)
            {
                for (int row = 0; row < GameConstants.FormationRows; row++)
                {
                    var inv = _grid[col, row];
                    if (inv == null || !inv.IsAlive) continue;

                    float worldX = formationX + col * GameConstants.InvaderSpacingX;
                    if (worldX < GameConstants.FormationMarginX || worldX > GameConstants.ScreenWidth - GameConstants.FormationMarginX)
                        return true;
                }
            }
            return false;
        }

        void CheckInvasion()
        {
            float formationY = Entity.Transform.Position.Y;
            for (int row = 0; row < GameConstants.FormationRows; row++)
            {
                for (int col = 0; col < GameConstants.FormationColumns; col++)
                {
                    var inv = _grid[col, row];
                    if (inv == null || !inv.IsAlive) continue;

                    float worldY = formationY + row * GameConstants.InvaderSpacingY;
                    if (worldY >= GameConstants.InvasionLineY)
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

            for (int col = 0; col < GameConstants.FormationColumns; col++)
            {
                for (int row = 0; row < GameConstants.FormationRows; row++)
                {
                    var inv = _grid[col, row];
                    if (inv == null || !inv.IsAlive) continue;

                    float worldX = formationX + col * GameConstants.InvaderSpacingX;
                    float worldY = formationY + row * GameConstants.InvaderSpacingY;
                    var invBounds = new RectangleF(worldX - 35, worldY - 30, 70, 60);

                    var hits = Physics.BoxcastBroadphase(invBounds, 1 << PhysicsLayers.Shield);
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

            _fireTimer = _fireInterval + ((float)_rng.NextDouble() - 0.5f) * GameConstants.FireIntervalVariance * 2;

            var enemyBullets = Entity.Scene.FindEntitiesWithTag(Tags.EnemyBullet);
            if (enemyBullets.Count >= GameConstants.MaxEnemyBullets)
                return;

            int col = PickFiringColumn();
            if (col < 0) return;

            var bottomInvader = GetBottomAliveInColumn(col);
            if (bottomInvader == null) return;

            float formationX = Entity.Transform.Position.X;
            float formationY = Entity.Transform.Position.Y;
            var bulletPos = new Vector2(
                formationX + bottomInvader.Column * GameConstants.InvaderSpacingX,
                formationY + bottomInvader.Row * GameConstants.InvaderSpacingY + 20
            );

            BulletController.CreateBullet(Entity.Scene, bulletPos, isPlayerBullet: false);
        }

        int PickFiringColumn()
        {
            int attempts = 20;
            while (attempts-- > 0)
            {
                int col = _rng.Next(GameConstants.FormationColumns);
                if (GetBottomAliveInColumn(col) != null)
                    return col;
            }
            return -1;
        }

        InvaderData GetBottomAliveInColumn(int col)
        {
            for (int row = 0; row < GameConstants.FormationRows; row++)
            {
                var inv = _grid[col, row];
                if (inv != null && inv.IsAlive)
                    return inv;
            }
            return null;
        }
    }
}
