namespace SpaceInvaders
{
    public static class PhysicsLayers
    {
        public const int Player = 0;
        public const int Invader = 1;
        public const int PlayerBullet = 2;
        public const int EnemyBullet = 3;
        public const int Shield = 4;

        public static int Mask(params int[] layers)
        {
            int mask = 0;
            foreach (var layer in layers)
                mask |= 1 << layer;
            return mask;
        }
    }

    public static class Tags
    {
        public const int EnemyBullet = 0;
    }

    public enum InvaderType
    {
        Octopus,
        Crab,
        Squid
    }

    public static class Constants
    {
        public const int ScreenWidth = 960;
        public const int ScreenHeight = 720;

        public const int FormationColumns = 11;
        public const int FormationRows = 5;
        public const float InvaderSpacingX = 58f;
        public const float InvaderSpacingY = 50f;
        public const float FormationMarginX = 40f;
        public const float FormationDropDistance = 28f;
        public const float FormationStartY = 80f;
        public const float InvaderScale = 0.45f;

        public const float BaseFormationSpeed = 30f;
        public const float MaxFormationSpeed = 500f;
        public const int TotalInvaders = FormationColumns * FormationRows;

        public const float PlayerY = 660f;
        public const float PlayerSpeed = 280f;
        public const float PlayerMarginX = 24f;

        public const float PlayerBulletSpeed = 600f;
        public const float EnemyBulletSpeed = 320f;
        public const int MaxEnemyBullets = 3;
        public const float BaseFireInterval = 1.2f;
        public const float FireIntervalVariance = 0.4f;
        public const float FireIntervalWaveMultiplier = 0.95f;

        public const int StartingLives = 3;
        public const int ExtraLifeScore = 1500;
        public const float DeathDelay = 1.5f;
        public const float RespawnInvulnerability = 1.5f;

        public const float ShieldY = 560f;
        public const int ShieldCount = 4;
        public const int ShieldChunksX = 4;
        public const int ShieldChunksY = 4;
        public const float ShieldChunkW = 20f;
        public const float ShieldChunkH = 16f;
        public const float ShieldScale = 0.45f;

        public const float InvasionLineY = 640f;

        public const float UfoSpeed = 160f;
        public const float UfoMinSpawnTime = 20f;
        public const float UfoMaxSpawnTime = 30f;
        public static readonly int[] UfoScores = { 50, 100, 150, 200, 300 };

        public const float WaveTransitionDelay = 2.0f;
        public const float WaveSpeedMultiplier = 1.10f;

        public static int PointsForInvader(InvaderType type) => type switch
        {
            InvaderType.Squid => 30,
            InvaderType.Crab => 20,
            InvaderType.Octopus => 10,
            _ => 10
        };

        public static InvaderType InvaderTypeForRow(int row) => row switch
        {
            4 => InvaderType.Squid,
            2 or 3 => InvaderType.Crab,
            _ => InvaderType.Octopus
        };
    }
}
