using Nez;

namespace SuperMario
{
    public static class Audio
    {
        public static void PlaySfx(string path) => Core.GetGlobalManager<SfxManager>().Play(path);

        public static void PlayMusic(string path) => Core.GetGlobalManager<MusicManager>().Play(path);
    }
}
