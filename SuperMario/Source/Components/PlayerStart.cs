using Nez;
using Nez.Tiled;

namespace SuperMario
{
    // Marker for spawning the player in a level.
    // Always put one in a level, otherwise it will throw an error
    public class PlayerStart : Component
    {
        public static void Spawn(Scene scene, TmxObject obj)
        {
            var center = EntityFactory.GetCenter(obj);
            scene.CreateEntity("playerstart", center)
                .AddComponent(new PlayerStart());
        }
    }
}
