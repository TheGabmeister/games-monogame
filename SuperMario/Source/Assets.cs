

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
			public const string PlayerJump = @"Content/Sfx/player_jump.wav";
			public const string PlayerHit = @"Content/Sfx/player_hit.wav";
			public const string PlayerDie = @"Content/Sfx/player_die.wav";
			public const string PlayerFire = @"Content/Sfx/player_fire.wav";
			public const string PickupCoin = @"Content/Sfx/pickup_coin.wav";
			public const string PickupMushroom = @"Content/Sfx/pickup_mushroom.wav";
			public const string PickupOneUp = @"Content/Sfx/pickup_one_up.wav";
			public const string EnemyHit = @"Content/Sfx/enemy_hit.wav";
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
