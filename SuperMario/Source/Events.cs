using Nez.Systems;

namespace SuperMario
{
    public enum GameEvents
    {
        
    }

    public static class Events
    {
        public static readonly Emitter<GameEvents> Emitter = new Emitter<GameEvents>();
    }
}
