using Microsoft.Xna.Framework;

namespace Extended.Components
{
    public class Player
    {
        public int Speed = 100;
        public Vector2 Position;

        public Player(int speed, Vector2 position)
        {
            Speed = speed;
            Position = position;
        }
    }
}
