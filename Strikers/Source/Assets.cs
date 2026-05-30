namespace Strikers
{
    // Canonical content load paths, mirroring the Content/ folder tree and Content.mgcb.
    // Load paths are plain strings handed to Content.Load / SfxManager / MusicManager, so a
    // typo only fails at runtime (and the audio services silently ignore a missing sound) — keeping
    // them here, one const per asset, gives a single place to get them right. Add a const
    // alongside its siblings whenever you register a new asset in Content.mgcb.
    public static class Assets
    {
        public static class Sprites
        {
            public const string PlayerShip = "sprites/player/player_ship";
            public const string PlayerShipLeft = "sprites/player/player_ship_left";
            public const string PlayerShipRight = "sprites/player/player_ship_right";

            public const string BulletPlayer = "sprites/bullets/bullet_player";
            public const string BulletEnemyRound = "sprites/bullets/bullet_enemy_round";
            public const string BulletEnemyNeedle = "sprites/bullets/bullet_enemy_needle";

            public const string EnemyPopcorn = "sprites/enemies/enemy_popcorn";
            public const string EnemyFighter = "sprites/enemies/enemy_fighter";
            public const string EnemyGunship = "sprites/enemies/enemy_gunship";

            public const string PowerUpWeapon = "sprites/powerups/powerup_weapon";
            public const string PowerUpBomb = "sprites/powerups/powerup_bomb";
            public const string PowerUpScore = "sprites/powerups/powerup_score";

            public const string HudLifeIcon = "sprites/hud/hud_life_icon";
            public const string HudBombIcon = "sprites/hud/hud_bomb_icon";
        }

        public static class Backgrounds
        {
            public const string Tile = "backgrounds/bg_tile";
        }

        public static class Fonts
        {
            public const string Main = "fonts/main";
        }

        public static class Sfx
        {
            public const string PlayerShot = "audio/sfx/sfx_player_shot";
            public const string EnemyShot = "audio/sfx/sfx_enemy_shot";
            public const string EnemyHit = "audio/sfx/sfx_enemy_hit";
            public const string EnemyExplode = "audio/sfx/sfx_enemy_explode";
            public const string PlayerExplode = "audio/sfx/sfx_player_explode";
            public const string PowerUp = "audio/sfx/sfx_powerup";
            public const string PowerUpWeapon = "audio/sfx/sfx_powerup_weapon";
            public const string Bomb = "audio/sfx/sfx_bomb";
            public const string Graze = "audio/sfx/sfx_graze";
            public const string MenuMove = "audio/sfx/sfx_menu_move";
            public const string MenuSelect = "audio/sfx/sfx_menu_select";
            public const string StageClear = "audio/sfx/sfx_stage_clear";
        }

        public static class Music
        {
            public const string Title = "audio/music/music_title";
            public const string Stage = "audio/music/music_stage";
            public const string GameOver = "audio/music/music_gameover";
        }
    }
}
