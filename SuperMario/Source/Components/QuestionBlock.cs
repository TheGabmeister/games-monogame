using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class QuestionBlock : Component, IBumpable
    {
        readonly GameState _gameState;
        readonly PrototypeSpriteRenderer _renderer;
        bool _used;

        public QuestionBlock(GameState gameState, PrototypeSpriteRenderer renderer)
        {
            _gameState = gameState;
            _renderer = renderer;
        }

        public static void Spawn(Scene scene, TmxObject obj, GameState gameState)
        {
            var center = EntityFactory.GetCenter(obj);
            var block = scene.CreateEntity("questionblock", center);
            var renderer = block.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height));
            renderer.SetColor(Color.Gold);

            var collider = block.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.PickupBody);

            block.AddComponent(new BlockBump());
            block.AddComponent(new QuestionBlock(gameState, renderer));
        }

        public void OnBumped(PlayerController player)
        {
            if (_used)
                return;

            _used = true;
            Entity.GetComponent<BlockBump>()?.Bump();
            _renderer.SetColor(Color.Sienna);

            _gameState.AddScore(Coin.CoinValue);
            ScorePopup.Spawn(Entity.Scene, Entity.Position + new Vector2(0f, -16f), Coin.CoinValue);
            Audio.PlaySfx(Assets.Sfx.PickupCoin);
        }
    }
}
