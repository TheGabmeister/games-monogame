using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Nez.Systems;

namespace SpaceInvaders
{
    public static class Assets
    {
        // Sprites
        public static Texture2D Cannon { get; private set; }
        public static Texture2D Squid { get; private set; }
        public static Texture2D Crab { get; private set; }
        public static Texture2D Octopus { get; private set; }
        public static Texture2D Ufo { get; private set; }
        public static Texture2D ShieldChunk { get; private set; }
        public static Texture2D BulletPlayer { get; private set; }
        public static Texture2D BulletEnemy { get; private set; }

        // SFX
        public static SoundEffect Shoot { get; private set; }
        public static SoundEffect InvaderDeath { get; private set; }
        public static SoundEffect PlayerDeath { get; private set; }
        public static SoundEffect UfoHum { get; private set; }
        public static SoundEffect UfoScore { get; private set; }
        public static SoundEffect ShieldHit { get; private set; }
        public static SoundEffect MenuMove { get; private set; }
        public static SoundEffect MenuSelect { get; private set; }
        public static SoundEffect WaveStart { get; private set; }
        public static SoundEffect ExtraLife { get; private set; }

        // Bass rhythm
        public static SoundEffect[] BassNotes { get; private set; }

        public static void Load(NezContentManager content)
        {
            Cannon = content.LoadTexture("Content/sprites/player/cannon.png", true);
            Squid = content.LoadTexture("Content/sprites/invaders/squid_01.png", true);
            Crab = content.LoadTexture("Content/sprites/invaders/crab_01.png", true);
            Octopus = content.LoadTexture("Content/sprites/invaders/octopus_01.png", true);
            Ufo = content.LoadTexture("Content/sprites/invaders/ufo.png", true);
            ShieldChunk = content.LoadTexture("Content/sprites/shields/shield_chunk.png", true);
            BulletPlayer = content.LoadTexture("Content/sprites/effects/bullet_player.png", true);
            BulletEnemy = content.LoadTexture("Content/sprites/effects/bullet_enemy.png", true);

            Shoot = content.LoadSoundEffect("Content/audio/sfx/shoot.wav");
            InvaderDeath = content.LoadSoundEffect("Content/audio/sfx/invader_death.wav");
            PlayerDeath = content.LoadSoundEffect("Content/audio/sfx/player_death.wav");
            UfoHum = content.LoadSoundEffect("Content/audio/sfx/ufo_hum.wav");
            UfoScore = content.LoadSoundEffect("Content/audio/sfx/ufo_score.wav");
            ShieldHit = content.LoadSoundEffect("Content/audio/sfx/shield_hit.wav");
            MenuMove = content.LoadSoundEffect("Content/audio/sfx/menu_move.wav");
            MenuSelect = content.LoadSoundEffect("Content/audio/sfx/menu_select.wav");
            WaveStart = content.LoadSoundEffect("Content/audio/sfx/wave_start.wav");
            ExtraLife = content.LoadSoundEffect("Content/audio/sfx/extra_life.wav");

            BassNotes = new SoundEffect[4];
            for (int i = 0; i < 4; i++)
                BassNotes[i] = content.LoadSoundEffect($"Content/audio/sfx/bass/note_{i + 1}.wav");
        }

        public static Texture2D InvaderTexture(InvaderType type) => type switch
        {
            InvaderType.Squid => Squid,
            InvaderType.Crab => Crab,
            _ => Octopus
        };
    }
}
