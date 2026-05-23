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
        public const int PickupBody = 2;
        public const int Environment = 3;
        public const int Projectile = 4;
        public const int EnemyProjectile = 5;
        public const int PickupTrigger = 6;
        public const int LevelTrigger = 7;
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
        public const float StompBounceForce = -420f;
        public const float StompTopTolerance = 16f;
        public const float MovingPlatformDistance = 160f;
        public const float MovingPlatformSpeed = 80f;
        public const float BlockBumpDistance = 8f;
        public const float BlockBumpDuration = 0.18f;
        public const int BrickBreakScore = 50;
        public const float StarmanSpeed = 220f;
        public const float StarmanBounceForce = -420f;
        public const float StarmanInvincibleDuration = 10f;
        public const int StarmanPickupScore = 1000;
        public const float GoombaWalkSpeed = 80f;
        public const float GreenKoopaTroopaWalkSpeed = 80f;
        public const float RedKoopaTroopaWalkSpeed = 80f;
        public const float GreenKoopaParatroopaFlySpeed = 100f;
        public const float RedKoopaParatroopaFlySpeed = 100f;
        public const float BuzzyBeetleWalkSpeed = 80f;
        public const float SpinyWalkSpeed = 80f;
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
        public const float BlooperSpeed = 90f;
        public const float BlooperBobSpeed = 4f;
        public const float BlooperBobStrength = 24f;
        public const float BulletBillSpeed = 180f;
        public const float BulletBillLifetime = 5f;
        public const float BulletBillCannonFireInterval = 4f;
        public const float PodobooJumpSpeed = 520f;
        public const float PodobooGravity = 1200f;
        public const float PodobooRestDuration = 1f;
        public const int StartingLives = 3;
    }
}
