using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class BlockBump : Component, IUpdatable
    {
        Vector2 _homePosition;
        float _timer;

        public override void OnAddedToEntity()
        {
            _homePosition = Entity.Position;
        }

        public void Bump()
        {
            _timer = Constants.BlockBumpDuration;
        }

        public void Update()
        {
            if (_timer <= 0f)
                return;

            var elapsed = Constants.BlockBumpDuration - _timer;
            var phase = elapsed / Constants.BlockBumpDuration;
            var offset = -Mathf.Sin(phase * MathHelper.Pi) * Constants.BlockBumpDistance;

            _timer -= Time.DeltaTime;
            Entity.Position = _timer <= 0f
                ? _homePosition
                : _homePosition + new Vector2(0f, offset);
        }
    }
}
