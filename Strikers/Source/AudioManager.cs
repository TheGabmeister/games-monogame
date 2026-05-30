using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Extended
{
    // A plain (non-ECS) service for one-shot sound effects. Audio is global state,
    // not a per-entity thing, so it lives outside the ECS (see PLAN.md §6). Systems
    // call Play(name); missing sounds are silently ignored so development is never
    // blocked on audio that hasn't been authored yet.
    public class AudioManager
    {
        private readonly ContentManager _content;
        private readonly Dictionary<string, SoundEffect> _cache = new();

        public AudioManager(ContentManager content)
        {
            _content = content;
        }

        // Loads on first use and caches. Returns silently if the asset isn't there.
        public void Play(string name)
        {
            if (!_cache.TryGetValue(name, out var sound))
            {
                try
                {
                    sound = _content.Load<SoundEffect>(name);
                }
                catch (ContentLoadException)
                {
                    sound = null;
                }
                _cache[name] = sound;
            }

            sound?.Play();
        }
    }
}
