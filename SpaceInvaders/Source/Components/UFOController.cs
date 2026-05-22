using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;

namespace SpaceInvaders
{
    public class UFOController : Component, IUpdatable, ITriggerListener
    {
        readonly int _direction;
        static readonly System.Random _rng = new System.Random();
        SoundEffectInstance _humInstance;

        public UFOController(int direction)
        {
            _direction = direction;
        }

        public override void OnAddedToEntity()
        {
            _humInstance = Assets.UfoHum.CreateInstance();
            _humInstance.IsLooped = true;
            _humInstance.Volume = 0.5f;
            _humInstance.Play();
        }

        public override void OnRemovedFromEntity()
        {
            _humInstance?.Stop();
            _humInstance?.Dispose();
        }

        public void Update()
        {
            var pos = Entity.Transform.Position;
            pos.X += _direction * GameConstants.UfoSpeed * Time.DeltaTime;
            Entity.Transform.Position = pos;

            if (pos.X < -60 || pos.X > GameConstants.ScreenWidth + 60)
                Entity.Destroy();
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == (1 << PhysicsLayers.PlayerBullet))
            {
                int score = GameConstants.UfoScores[_rng.Next(GameConstants.UfoScores.Length)];
                var gameState = Entity.Scene.GetSceneComponent<GameState>();
                gameState?.AddScore(score);
                Assets.UfoScore.Play();
                Entity.Destroy();
            }
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
