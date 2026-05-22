using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class UFOController : Component, IUpdatable, ITriggerListener
    {
        readonly int _direction;
        static readonly System.Random _rng = new System.Random();
        SoundEffectInstance _humInstance;
        SoundEffect _ufoScore;

        public UFOController(int direction)
        {
            _direction = direction;
        }

        public override void OnAddedToEntity()
        {
            var ufoHum = Entity.Scene.Content.LoadSoundEffect(Assets.Audio.Sfx.UfoHum);
            _humInstance = ufoHum.CreateInstance();
            _humInstance.IsLooped = true;
            _humInstance.Volume = 0.5f;
            _humInstance.Play();
            _ufoScore = Entity.Scene.Content.LoadSoundEffect(Assets.Audio.Sfx.UfoScore);
        }

        public override void OnRemovedFromEntity()
        {
            _humInstance?.Stop();
            _humInstance?.Dispose();
        }

        public void Update()
        {
            var pos = Entity.Transform.Position;
            pos.X += _direction * Constants.UfoSpeed * Time.DeltaTime;
            Entity.Transform.Position = pos;

            if (pos.X < -60 || pos.X > Constants.ScreenWidth + 60)
                Entity.Destroy();
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == (1 << PhysicsLayers.PlayerBullet))
            {
                int score = Constants.UfoScores[_rng.Next(Constants.UfoScores.Length)];
                var gameState = Entity.Scene.GetSceneComponent<GameState>();
                gameState?.AddScore(score);
                _ufoScore.Play();
                Entity.Destroy();
            }
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
