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
        public const int StartingLives = 3;
    }
}
