public static class GameConstants
{
    public const int VirtualWidth = 960;
    public const int VirtualHeight = 720;

    public const int StartingLives = 3;
    public const int MaxPlayerBullets = 4;

    public const float ShipCollisionRadius = 16f;
    public const float BulletCollisionRadius = 3f;

    public const float BulletSpeed = 520f;
    public const float BulletLifetime = 1.1f;
    public const float FireCooldown = 0.16f;

    public const float BulletSpawnOffset = 26f;
    public const float BulletVelocityInheritance = 0.35f;
    public const float ThrusterOffset = 24f;
    public const float ThrusterScale = 0.55f;

    public const float DeathDelay = 1.25f;
    public const float RespawnRetryDelay = 0.25f;
    public const float RespawnInvulnerability = 2f;
    public const float SafeRespawnRadius = 140f;
    public const float WaveTransitionDelay = 1.4f;
    public const int MaxSpawnAttempts = 40;

    public const float HudX = 18f;
    public const float HudY = 16f;
    public const float PauseHintX = 764f;
    public const float WaveTextY = 105f;
    public const float TitleY = 275f;
    public const float SubtitleStartY = 325f;
    public const float TextLineSpacing = 28f;
}
