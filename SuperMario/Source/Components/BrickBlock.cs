using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class BrickBlock : Component, IBumpable
    {
        readonly GameState _gameState;

        public BrickBlock(GameState gameState)
        {
            _gameState = gameState;
        }

        public static void Spawn(Scene scene, TmxObject obj, GameState gameState)
        {
            var center = EntityFactory.GetCenter(obj);
            var block = scene.CreateEntity("brickblock", center);
            block.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.SaddleBrown);

            var collider = block.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.PickupBody);

            block.AddComponent(new BlockBump());
            block.AddComponent(new BrickBlock(gameState));
        }

        public void OnBumped(PlayerController player)
        {
            if (!player.CanBreakBricks)
            {
                Entity.GetComponent<BlockBump>()?.Bump();
                Audio.PlaySfx(Assets.Sfx.PlayerFireHitBlock);
                return;
            }

            _gameState.AddScore(Constants.BrickBreakScore);
            ScorePopup.Spawn(Entity.Scene, Entity.Position, Constants.BrickBreakScore);
            Audio.PlaySfx(Assets.Sfx.PlayerFireHitBlock);
            Entity.Destroy();
        }
    }
}
