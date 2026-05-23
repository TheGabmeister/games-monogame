namespace SuperMario
{
    public enum PlayerState
    {
        Small,
        Big,
        Fire
    }

    public static class PhysicsLayers
    {
        public const int Player = 0;
        public const int Enemy = 1;
        public const int Item = 2;
        public const int Environment = 3;
        public const int Projectile = 4;
        public const int EnemyProjectile = 5;
    }

    // In Nez, 0 is the default tag for entities. Don't use it.
    public static class Tags
    {
        public const int PlayerStart = 1;
    }

    public static class RenderLayers
    {
        public const int World = 0;
        public const int Hud = 1;
    }

    public static class Constants
    {
        public const int ScreenWidth = 960;
        public const int ScreenHeight = 720;
        public const float PlayerSpeed = 280f;
        public const float GoombaWalkSpeed = 80f;
        public const float GreenKoopaTroopaWalkSpeed = 80f;
        public const float RedKoopaTroopaWalkSpeed = 80f;
        public const float GreenKoopaParatroopaFlySpeed = 100f;
        public const float RedKoopaParatroopaFlySpeed = 100f;
        public const float BuzzyBeetleWalkSpeed = 80f;
        public const float HammerBroShuffleSpeed = 40f;
        public const float HammerBroShuffleInterval = 1.2f;
        public const float HammerBroJumpForce = -520f;
        public const float HammerBroJumpInterval = 3.5f;
        public const float HammerBroThrowInterval = 2.0f;
        public const float HammerHorizontalSpeed = 220f;
        public const float HammerInitialUpSpeed = 520f;
        public const float HammerGravity = 1400f;
        public const float HammerLifetime = 3f;
        public const float HammerSpinSpeed = 18f;
        public const int StartingLives = 3;
    }
}
