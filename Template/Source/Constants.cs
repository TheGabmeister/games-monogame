namespace Template
{
    public static class PhysicsLayers
    {
        public const int Player = 0;
        public const int Enemy = 1;

        // convenience helper. Combines multiple layer indices into a single bitmask. 
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
