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
    }

    public static class Tags
    {
        public const int PlayerStart = 0;
    }

    public static class Constants
    {
        public const int ScreenWidth = 960;
        public const int ScreenHeight = 720;
        public const float PlayerSpeed = 280f;
        public const int StartingLives = 3;
    }
}
