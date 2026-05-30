using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Strikers
{
    // A plain (non-ECS) service for one-shot sound effects — the short, layerable arcade
    // SFX that fire many-at-once during dense play. Audio is global state, not a per-entity
    // thing, so it lives outside the ECS (see PLAN.md §6). Systems call Play(name); missing
    // sounds are silently ignored so development is never blocked on audio that hasn't been
    // authored yet. Looping background music is the separate MusicManager (it streams
    // through MediaPlayer, a different API).
    public class SfxManager
    {
        private readonly ContentManager _content;
        private readonly Dictionary<string, SoundEffect> _cache = new();

        public SfxManager(ContentManager content)
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
