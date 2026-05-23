using Nez;
using Nez.Tiled;

namespace SuperMario
{
    public class KillVolume : Component, ITriggerListener
    {
        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            var killVolume = scene.CreateEntity("killvolume", center);
            var collider = killVolume.AddComponent(new BoxCollider(-obj.Width / 2f, -obj.Height / 2f, obj.Width, obj.Height));
            collider.PhysicsLayer = 1 << PhysicsLayers.Environment;
            collider.CollidesWithLayers = 1 << PhysicsLayers.Player;
            collider.IsTrigger = true;
            killVolume.AddComponent(new KillVolume());
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            var player = other.Entity.GetComponent<PlayerController>();
            if (player != null)
                player.KillPlayer();
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
