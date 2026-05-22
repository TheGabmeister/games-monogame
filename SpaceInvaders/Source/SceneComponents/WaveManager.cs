using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using Nez.Tweens;

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
                Scene.GetSceneComponent<EventBus>().Emitter.Emit(GameEvents.WaveCleared);
                ShowWaveText(_gameState.Wave + 1);
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
                    _gameState.AdvanceWave();
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
            Flags.SetFlagExclusive(ref collider.PhysicsLayer, PhysicsLayers.Invader);
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
            var controller = formationEntity.AddComponent<FormationController>();
            controller.SetWave(wave);

            for (int row = 0; row < Constants.FormationRows; row++)
            {
                var type = Constants.InvaderTypeForRow(row);
                var frames = InvaderFrames(type);

                for (int col = 0; col < Constants.FormationColumns; col++)
                {
                    var localPos = new Vector2(col * Constants.InvaderSpacingX, row * Constants.InvaderSpacingY);
                    var invader = Scene.CreateEntity($"invader_{col}_{row}");
                    invader.Transform.SetParent(formationEntity.Transform);
                    invader.Transform.LocalPosition = localPos;

                    var animator = invader.AddComponent<SpriteAnimator>();
                    animator.AddAnimation("idle", Constants.InvaderBaseAnimFps, frames[0], frames[1]);
                    animator.Play("idle");
                    invader.Transform.SetScale(Constants.InvaderScale);

                    var collider = invader.AddComponent(new BoxCollider(50, 40));
                    Flags.SetFlagExclusive(ref collider.PhysicsLayer, PhysicsLayers.Invader);
                    collider.CollidesWithLayers = 0;

                    var data = invader.AddComponent(new InvaderData(type, col, row));
                    controller.SetInvader(col, row, data);
                }
            }

            _formation = controller;
        }

        void ShowWaveText(int wave)
        {
            var font = Graphics.Instance.BitmapFont;
            var center = new Vector2(Constants.ScreenWidth / 2f, Constants.ScreenHeight / 2f);
            var entity = Scene.CreateEntity("wave-text", center);
            var text = entity.AddComponent(new TextComponent(font, $"WAVE {wave}", Vector2.Zero, Color.LightGreen));
            text.SetHorizontalAlign(HorizontalAlign.Center);
            text.SetVerticalAlign(VerticalAlign.Center);
            entity.Transform.SetScale(0.01f);

            entity.TweenScaleTo(5f, 0.5f)
                .SetEaseType(EaseType.BackOut)
                .SetCompletionHandler(_ =>
                {
                    text.TweenColorTo(Color.Transparent, 0.3f)
                        .SetDelay(1.0f)
                        .SetCompletionHandler(__ => { if (!entity.IsDestroyed) entity.Destroy(); })
                        .Start();
                })
                .Start();
        }

        Sprite[] InvaderFrames(InvaderType type)
        {
            var (path1, path2) = type switch
            {
                InvaderType.Squid => (Assets.Sprites.Invaders.Squid01, Assets.Sprites.Invaders.Squid02),
                InvaderType.Crab => (Assets.Sprites.Invaders.Crab01, Assets.Sprites.Invaders.Crab02),
                _ => (Assets.Sprites.Invaders.Octopus01, Assets.Sprites.Invaders.Octopus02)
            };
            return new[]
            {
                new Sprite(Scene.Content.LoadTexture(path1, true)),
                new Sprite(Scene.Content.LoadTexture(path2, true))
            };
        }
    }
}
