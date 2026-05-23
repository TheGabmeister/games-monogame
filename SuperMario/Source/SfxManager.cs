using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Nez;

namespace SuperMario
{
    public class SfxManager : GlobalManager
    {
        readonly Dictionary<string, SoundEffect> _cache = new();

        public void Play(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!_cache.TryGetValue(path, out var sfx))
            {
                using var stream = new FileStream(Path.GetFullPath(path), FileMode.Open, FileAccess.Read);
                sfx = SoundEffect.FromStream(stream);
                _cache[path] = sfx;
            }

            sfx.Play();
        }
    }
}
