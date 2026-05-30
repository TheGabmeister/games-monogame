using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Strikers
{
    // A plain (non-ECS) service for background music — the looping/streamed counterpart to
    // SfxManager's one-shots (see PLAN.md §6/§10). Songs play through MediaPlayer rather
    // than SoundEffect, so only one track sounds at a time and starting another replaces it.
    // Loads on demand and caches; an unauthored song is silently ignored so playback never
    // blocks development. Screens/flow drive this (title vs stage vs game-over music), not
    // gameplay systems.
    public class MusicManager
    {
        private readonly ContentManager _content;
        private readonly Dictionary<string, Song> _cache = new();
        private string _current;

        public MusicManager(ContentManager content)
        {
            _content = content;
        }

        // Streams `name`, replacing whatever's playing. A no-op when that track is already
        // playing, so callers can call it on every screen-enter without restarting it.
        public void Play(string name, bool loop = true)
        {
            if (name == _current && MediaPlayer.State == MediaState.Playing)
                return;

            _current = name;
            var song = Load(name);
            if (song == null)
            {
                MediaPlayer.Stop();
                return;
            }

            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Play(song);
        }

        public void Stop()
        {
            _current = null;
            MediaPlayer.Stop();
        }

        private Song Load(string name)
        {
            if (!_cache.TryGetValue(name, out var song))
            {
                try
                {
                    song = _content.Load<Song>(name);
                }
                catch (ContentLoadException)
                {
                    song = null;
                }
                _cache[name] = song;
            }
            return song;
        }
    }
}
