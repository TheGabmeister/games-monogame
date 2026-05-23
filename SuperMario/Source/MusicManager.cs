using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Nez;

namespace SuperMario
{
    public class MusicManager : GlobalManager
    {
        string _currentPath;
        Song _currentSong;

        public void Play(string path)
        {
            if (path == _currentPath)
                return;

            Stop();

            if (string.IsNullOrEmpty(path))
                return;

            _currentPath = path;
            var uri = new Uri(Path.GetFullPath(path));
            _currentSong = Song.FromUri(path, uri);

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(_currentSong);
        }

        public void Stop()
        {
            MediaPlayer.Stop();
            _currentSong?.Dispose();
            _currentSong = null;
            _currentPath = null;
        }
    }
}
