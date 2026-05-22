using Nez;

namespace SpaceInvaders
{
    public class ShakeListener : Component
    {
        CameraShake _cameraShake;
        EventBus _eventBus;

        public override void OnAddedToEntity()
        {
            _cameraShake = Entity.GetComponent<CameraShake>();
            _eventBus = Entity.Scene.GetSceneComponent<EventBus>();

            _eventBus.Emitter.AddObserver(GameEvents.PlayerDied, OnPlayerDied);
            _eventBus.Emitter.AddObserver(GameEvents.UfoDestroyed, OnUfoDestroyed);
        }

        public override void OnRemovedFromEntity()
        {
            _eventBus.Emitter.RemoveObserver(GameEvents.PlayerDied, OnPlayerDied);
            _eventBus.Emitter.RemoveObserver(GameEvents.UfoDestroyed, OnUfoDestroyed);
        }

        void OnPlayerDied()
        {
            if (Settings.Instance.ScreenShake)
                _cameraShake.Shake(15f, 0.9f);
        }

        void OnUfoDestroyed()
        {
            if (Settings.Instance.ScreenShake)
                _cameraShake.Shake(8f, 0.9f);
        }
    }
}
