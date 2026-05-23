using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public static class UsedBlock
    {
        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var block = scene.CreateEntity("usedblock", center);
            block.AddComponent(new PrototypeSpriteRenderer(obj.Width, obj.Height)).SetColor(Color.Sienna);

            var collider = block.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = (1 << PhysicsLayers.Player) | (1 << PhysicsLayers.PickupBody);
        }
    }
}
