using Nez;

namespace SuperMario
{
    public class Lifetime : Component, IUpdatable
    {
        readonly float _duration;
        float _age;

        public Lifetime(float duration)
        {
            _duration = duration;
        }

        public void Update()
        {
            _age += Time.DeltaTime;
            if (_age >= _duration)
                Entity.Destroy();
        }
    }
}
