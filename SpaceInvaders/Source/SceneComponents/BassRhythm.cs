using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public class BassRhythm : SceneComponent
    {
        const float MaxInterval = 0.6f;
        const float MinInterval = 0.08f;

        WaveManager _waveManager;
        SoundEffect[] _bassNotes;
        int _noteIndex;
        float _timer;
        bool _playing;

        public override void OnEnabled()
        {
            _waveManager = Scene.GetSceneComponent<WaveManager>();
            _bassNotes = new[]
            {
                Core.Content.LoadSoundEffect(ContentPaths.Audio.Sfx.Bass.Note1),
                Core.Content.LoadSoundEffect(ContentPaths.Audio.Sfx.Bass.Note2),
                Core.Content.LoadSoundEffect(ContentPaths.Audio.Sfx.Bass.Note3),
                Core.Content.LoadSoundEffect(ContentPaths.Audio.Sfx.Bass.Note4),
            };
            _playing = true;
            _timer = MaxInterval;
        }

        public override void Update()
        {
            if (!_playing) return;

            var gameState = Scene.GetSceneComponent<GameState>();
            if (gameState.IsGameOver)
                return;

            _timer -= Time.DeltaTime;
            if (_timer > 0) return;

            float ratio = (float)_waveManager.AliveCount / GameConstants.TotalInvaders;
            float interval = MinInterval + (MaxInterval - MinInterval) * ratio;
            _timer = interval;

            _bassNotes[_noteIndex].Play();
            _noteIndex = (_noteIndex + 1) % 4;
        }
    }
}
