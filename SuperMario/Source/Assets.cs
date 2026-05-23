

namespace SuperMario
{
    /// <summary>
    /// class that contains the names of all of the files processed by the Pipeline Tool
    /// </summary>
    /// <remarks>
    /// Nez includes a T4 template that will auto-generate the content of this file.
    /// See: https://github.com/prime31/Nez/blob/master/FAQs/ContentManagement.md#auto-generating-content-paths"
    /// </remarks>
    class Assets
    {
		public static class Maps
		{
			public const string Debug = @"Content/Levels/Debug.tmx";
			public const string World1_1 = @"Content/Levels/World_1_1.tmx";
			public const string World1_2 = @"Content/Levels/World_1_2.tmx";
			public const string World1_3 = @"Content/Levels/World_1_3.tmx";
		}

		public static class Sprites
		{
		}

		public static class Sfx
		{
		}

		public static class Music
		{
			public const string MainMenu = @"Content/Music/main_menu.ogg";
			public const string GameOver = @"Content/Music/game_over.ogg";
			public const string LevelBounce = @"Content/Music/level_bounce.ogg";
			public const string LevelCavern = @"Content/Music/level_cavern.ogg";
			public const string LevelSky = @"Content/Music/level_sky.ogg";
		}
    }
}
