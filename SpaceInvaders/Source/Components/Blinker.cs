using Nez;
using Nez.Sprites;

namespace SpaceInvaders
{
    public class Blinker : Component, IUpdatable
    {
        readonly float _blinkRate;
        float _timer;
        SpriteRenderer _renderer;

        public Blinker(float blinkRate = 10f)
        {
            _blinkRate = blinkRate;
        }

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
        }

        public override void OnDisabled()
        {
            _timer = 0;
            if (_renderer != null)
                _renderer.Enabled = true;
        }

        public override void OnRemovedFromEntity()
        {
            if (_renderer != null)
                _renderer.Enabled = true;
        }

        public void Update()
        {
            _timer += Time.DeltaTime;
            if (_renderer != null)
                _renderer.Enabled = ((int)(_timer * _blinkRate) % 2) == 0;
        }
    }
}
