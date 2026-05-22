using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace SpaceInvaders
{
    public class WaveManager : SceneComponent
    {
        FormationController _formation;
        GameState _gameState;
        float _waveTransitionTimer;
        bool _transitioning;
        float _ufoTimer;
        System.Random _rng = new System.Random();
        int _ufoDirection = 1;

        public int AliveCount => _formation?.AliveCount ?? 0;

        public override void OnEnabled()
        {
            _gameState = Scene.GetSceneComponent<GameState>();
            ResetUfoTimer();
        }

        public void SetFormation(FormationController formation)
        {
            _formation = formation;
        }

        public void OnInvaderKilled()
        {
            if (_formation != null && _formation.AliveCount <= 0 && !_transitioning)
            {
                _transitioning = true;
                _waveTransitionTimer = GameConstants.WaveTransitionDelay;
            }
        }

        public override void Update()
        {
            if (_gameState.IsGameOver) return;

            if (_transitioning)
            {
                _waveTransitionTimer -= Time.DeltaTime;
                if (_waveTransitionTimer <= 0)
                {
                    _transitioning = false;
                    _gameState.Wave++;
                    SpawnFormation();
                }
                return;
            }

            UpdateUfo();
        }

        void UpdateUfo()
        {
            _ufoTimer -= Time.DeltaTime;
            if (_ufoTimer > 0) return;

            ResetUfoTimer();
            SpawnUfo();
        }

        void ResetUfoTimer()
        {
            _ufoTimer = GameConstants.UfoMinSpawnTime +
                (float)_rng.NextDouble() * (GameConstants.UfoMaxSpawnTime - GameConstants.UfoMinSpawnTime);
        }

        void SpawnUfo()
        {
            _ufoDirection *= -1;
            float startX = _ufoDirection > 0 ? -48 : GameConstants.ScreenWidth + 48;
            var ufo = Scene.CreateEntity("ufo", new Vector2(startX, 40));

            var texture = Scene.Content.LoadTexture("Content/sprites/invaders/ufo.png", true);
            ufo.AddComponent(new SpriteRenderer(texture));
            ufo.Transform.SetScale(0.5f);

            var collider = ufo.AddComponent(new BoxCollider(48, 20));
            collider.PhysicsLayer = 1 << PhysicsLayers.Invader;
            collider.CollidesWithLayers = 0;

            ufo.AddComponent(new UFOController(_ufoDirection));
        }

        public void SpawnFormation()
        {
            if (_formation != null && _formation.Entity != null && !_formation.Entity.IsDestroyed)
            {
                for (int i = _formation.Entity.Transform.ChildCount - 1; i >= 0; i--)
                    _formation.Entity.Transform.GetChild(i).Entity.Destroy();
                _formation.Entity.Destroy();
            }

            int wave = _gameState.Wave;
            float startY = GameConstants.FormationStartY + (wave - 1) * GameConstants.FormationDropDistance;
            startY = MathHelper.Min(startY, GameConstants.ShieldY - GameConstants.FormationRows * GameConstants.InvaderSpacingY - 40);

            float formationWidth = (GameConstants.FormationColumns - 1) * GameConstants.InvaderSpacingX;
            float startX = (GameConstants.ScreenWidth - formationWidth) / 2f;

            var formationEntity = Scene.CreateEntity("formation", new Vector2(startX, startY));
            formationEntity.Tag = Tags.Formation;
            var controller = formationEntity.AddComponent<FormationController>();
            controller.SetWave(wave);

            var squidTex = Scene.Content.LoadTexture("Content/sprites/invaders/squid_01.png", true);
            var crabTex = Scene.Content.LoadTexture("Content/sprites/invaders/crab_01.png", true);
            var octopusTex = Scene.Content.LoadTexture("Content/sprites/invaders/octopus_01.png", true);

            for (int row = 0; row < GameConstants.FormationRows; row++)
            {
                var type = GameConstants.InvaderTypeForRow(row);
                var texture = type switch
                {
                    InvaderType.Squid => squidTex,
                    InvaderType.Crab => crabTex,
                    _ => octopusTex
                };

                for (int col = 0; col < GameConstants.FormationColumns; col++)
                {
                    var localPos = new Vector2(col * GameConstants.InvaderSpacingX, row * GameConstants.InvaderSpacingY);
                    var invader = Scene.CreateEntity($"invader_{col}_{row}");
                    invader.Transform.SetParent(formationEntity.Transform);
                    invader.Transform.LocalPosition = localPos;
                    invader.Tag = Tags.Invader;

                    invader.AddComponent(new SpriteRenderer(texture));
                    invader.Transform.SetScale(GameConstants.InvaderScale);

                    var collider = invader.AddComponent(new BoxCollider(50, 40));
                    collider.PhysicsLayer = 1 << PhysicsLayers.Invader;
                    collider.CollidesWithLayers = 0;

                    var data = invader.AddComponent(new InvaderData(type, col, row));
                    controller.SetInvader(col, row, data);
                }
            }

            _formation = controller;
        }
    }
}
