using Nez;

namespace SuperMario
{
    public class Blinker : Component, IUpdatable
    {
        readonly RenderableComponent _renderer;
        readonly float _interval;

        bool _blinking;
        float _remaining;
        float _nextToggle;

        public Blinker(RenderableComponent renderer, float interval = 0.08f)
        {
            _renderer = renderer;
            _interval = interval;
        }

        public void Blink(float duration)
        {
            _blinking = true;
            _remaining = duration;
            _nextToggle = _interval;
        }

        public void Update()
        {
            if (!_blinking)
                return;

            _remaining -= Time.DeltaTime;
            if (_remaining <= 0)
            {
                _blinking = false;
                _renderer.Enabled = true;
                return;
            }

            _nextToggle -= Time.DeltaTime;
            if (_nextToggle <= 0)
            {
                _renderer.Enabled = !_renderer.Enabled;
                _nextToggle = _interval;
            }
        }
    }
}
