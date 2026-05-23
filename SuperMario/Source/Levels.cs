namespace SuperMario
{
    public class LevelDefinition
    {
        public string Name { get; init; }
        public string MapPath { get; init; }
        public string MusicPath { get; init; }
        public float TimerSeconds { get; init; }
    }

    public static class Levels
    {
        public static readonly LevelDefinition Debug = new()
        {
            Name = "Debug",
            MapPath = Assets.Maps.Debug,
            MusicPath = null,
            TimerSeconds = 300f,
        };
    }
}
