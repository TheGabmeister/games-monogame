using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Systems;

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
        SoundEffect _waveStart;

        public int AliveCount => _formation?.AliveCount ?? 0;

        public override void OnEnabled()
        {
            _gameState = Scene.GetSceneComponent<GameState>();
            _waveStart = Scene.Content.LoadSoundEffect(Assets.Audio.Sfx.WaveStart);
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
                _waveTransitionTimer = Constants.WaveTransitionDelay;
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
                    _waveStart.Play();
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
            _ufoTimer = Constants.UfoMinSpawnTime +
                (float)_rng.NextDouble() * (Constants.UfoMaxSpawnTime - Constants.UfoMinSpawnTime);
        }

        void SpawnUfo()
        {
            _ufoDirection *= -1;
            float startX = _ufoDirection > 0 ? -48 : Constants.ScreenWidth + 48;
            var ufo = Scene.CreateEntity("ufo", new Vector2(startX, 40));

            var ufoTex = Scene.Content.LoadTexture(Assets.Sprites.Invaders.Ufo, true);
            ufo.AddComponent(new SpriteRenderer(ufoTex));
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
            float startY = Constants.FormationStartY + (wave - 1) * Constants.FormationDropDistance;
            startY = MathHelper.Min(startY, Constants.ShieldY - Constants.FormationRows * Constants.InvaderSpacingY - 40);

            float formationWidth = (Constants.FormationColumns - 1) * Constants.InvaderSpacingX;
            float startX = (Constants.ScreenWidth - formationWidth) / 2f;

            var formationEntity = Scene.CreateEntity("formation", new Vector2(startX, startY));
            formationEntity.Tag = Tags.Formation;
            var controller = formationEntity.AddComponent<FormationController>();
            controller.SetWave(wave);

            for (int row = 0; row < Constants.FormationRows; row++)
            {
                var type = Constants.InvaderTypeForRow(row);
                var texture = InvaderTexture(type);

                for (int col = 0; col < Constants.FormationColumns; col++)
                {
                    var localPos = new Vector2(col * Constants.InvaderSpacingX, row * Constants.InvaderSpacingY);
                    var invader = Scene.CreateEntity($"invader_{col}_{row}");
                    invader.Transform.SetParent(formationEntity.Transform);
                    invader.Transform.LocalPosition = localPos;
                    invader.Tag = Tags.Invader;

                    invader.AddComponent(new SpriteRenderer(texture));
                    invader.Transform.SetScale(Constants.InvaderScale);

                    var collider = invader.AddComponent(new BoxCollider(50, 40));
                    collider.PhysicsLayer = 1 << PhysicsLayers.Invader;
                    collider.CollidesWithLayers = 0;

                    var data = invader.AddComponent(new InvaderData(type, col, row));
                    controller.SetInvader(col, row, data);
                }
            }

            _formation = controller;
        }

        Texture2D InvaderTexture(InvaderType type)
        {
            var path = type switch
            {
                InvaderType.Squid => Assets.Sprites.Invaders.Squid01,
                InvaderType.Crab => Assets.Sprites.Invaders.Crab01,
                _ => Assets.Sprites.Invaders.Octopus01
            };
            return Scene.Content.LoadTexture(path, true);
        }
    }
}
