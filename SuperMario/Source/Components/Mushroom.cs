using Nez;

namespace SuperMario
{
    public class Mushroom : Component, ITriggerListener
    {
        readonly PlayerController _playerController;

        public Mushroom(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            _playerController.GrowPlayer();
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
