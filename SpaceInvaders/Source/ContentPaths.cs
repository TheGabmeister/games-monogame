namespace SpaceInvaders
{
    static class ContentPaths
    {
        public static class Sprites
        {
            public static class Player
            {
                public const string Cannon = @"Content/sprites/player/cannon.png";
            }

            public static class Invaders
            {
                public const string Squid01 = @"Content/sprites/invaders/squid_01.png";
                public const string Squid02 = @"Content/sprites/invaders/squid_02.png";
                public const string Crab01 = @"Content/sprites/invaders/crab_01.png";
                public const string Crab02 = @"Content/sprites/invaders/crab_02.png";
                public const string Octopus01 = @"Content/sprites/invaders/octopus_01.png";
                public const string Octopus02 = @"Content/sprites/invaders/octopus_02.png";
                public const string Ufo = @"Content/sprites/invaders/ufo.png";
            }

            public static class Shields
            {
                public const string ShieldChunk = @"Content/sprites/shields/shield_chunk.png";
            }

            public static class Effects
            {
                public const string BulletPlayer = @"Content/sprites/effects/bullet_player.png";
                public const string BulletEnemy = @"Content/sprites/effects/bullet_enemy.png";
                public const string ExplosionParticle = @"Content/sprites/effects/explosion_particle.png";
                public const string MuzzleFlash = @"Content/sprites/effects/muzzle_flash.png";
            }
        }

        public static class Audio
        {
            public static class Sfx
            {
                public const string Shoot = @"Content/audio/sfx/shoot.wav";
                public const string InvaderDeath = @"Content/audio/sfx/invader_death.wav";
                public const string PlayerDeath = @"Content/audio/sfx/player_death.wav";
                public const string UfoHum = @"Content/audio/sfx/ufo_hum.wav";
                public const string UfoScore = @"Content/audio/sfx/ufo_score.wav";
                public const string ShieldHit = @"Content/audio/sfx/shield_hit.wav";
                public const string MenuMove = @"Content/audio/sfx/menu_move.wav";
                public const string MenuSelect = @"Content/audio/sfx/menu_select.wav";
                public const string WaveStart = @"Content/audio/sfx/wave_start.wav";
                public const string ExtraLife = @"Content/audio/sfx/extra_life.wav";

                public static class Bass
                {
                    public const string Note1 = @"Content/audio/sfx/bass/note_1.wav";
                    public const string Note2 = @"Content/audio/sfx/bass/note_2.wav";
                    public const string Note3 = @"Content/audio/sfx/bass/note_3.wav";
                    public const string Note4 = @"Content/audio/sfx/bass/note_4.wav";
                }
            }
        }
    }
}
