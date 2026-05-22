using Nez;
using Nez.Systems;

namespace SpaceInvaders
{
    public enum GameEvents
    {
        PlayerDied,
        UfoDestroyed,
        WaveCleared
    }

    public class EventBus : SceneComponent
    {
        public readonly Emitter<GameEvents> Emitter = new Emitter<GameEvents>();
    }
}
