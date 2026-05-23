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
            MusicPath = Assets.Music.LevelBounce,
            TimerSeconds = 300f,
        };

        public static readonly LevelDefinition World1_1 = new()
        {
            Name = "World 1-1",
            MapPath = Assets.Maps.World1_1,
            MusicPath = Assets.Music.LevelBounce,
            TimerSeconds = 400f,
        };

        public static readonly LevelDefinition World1_2 = new()
        {
            Name = "World 1-2",
            MapPath = Assets.Maps.World1_2,
            MusicPath = Assets.Music.LevelCavern,
            TimerSeconds = 400f,
        };

        public static readonly LevelDefinition World1_3 = new()
        {
            Name = "World 1-3",
            MapPath = Assets.Maps.World1_3,
            MusicPath = Assets.Music.LevelSky,
            TimerSeconds = 400f,
        };
    }
}
