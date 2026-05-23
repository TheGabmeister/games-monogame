using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class GoalTrigger : Component, ITriggerListener
    {
        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var goal = scene.CreateEntity("goaltrigger", center);
            goal.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.Gold);

            var collider = goal.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.LevelTrigger;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Player;
            collider.IsTrigger = true;

            goal.AddComponent(new GoalTrigger());
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player == null)
                return;

            if (Entity.Scene is GameplayScene gameplayScene)
                gameplayScene.CompleteLevel();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
