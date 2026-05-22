using Nez;
using Nez.Sprites;

namespace SpaceInvaders
{
    public class InvaderData : Component, ITriggerListener
    {
        public InvaderType Type;
        public int Points;
        public int Column;
        public int Row;
        public bool IsAlive = true;

        public InvaderData(InvaderType type, int col, int row)
        {
            Type = type;
            Points = GameConstants.PointsForInvader(type);
            Column = col;
            Row = row;
        }

        public void Kill()
        {
            if (!IsAlive) return;
            IsAlive = false;
            Assets.InvaderDeath.Play();

            var renderer = Entity.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.Enabled = false;

            var collider = Entity.GetComponent<Collider>();
            if (collider != null)
                collider.Enabled = false;

            var gameState = Entity.Scene.GetSceneComponent<GameState>();
            gameState?.AddScore(Points);

            var waveManager = Entity.Scene.GetSceneComponent<WaveManager>();
            waveManager?.OnInvaderKilled();
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.PhysicsLayer == (1 << PhysicsLayers.PlayerBullet))
                Kill();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
