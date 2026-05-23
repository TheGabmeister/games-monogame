using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class ScorePopup : Component, IUpdatable
    {
        const float Duration = 0.75f;
        const float RiseSpeed = 48f;

        TextComponent _text;
        float _elapsed;

        public ScorePopup(int points)
        {
            Points = points;
        }

        public int Points { get; }

        public static void Spawn(Scene scene, Vector2 position, int points)
        {
            if (points <= 0) return;

            var popup = scene.CreateEntity("scorepopup", position + new Vector2(0, -24));
            popup.Scale = new Vector2(1.5f);
            popup.AddComponent(new TextComponent())
                .SetText(points.ToString())
                .SetHorizontalAlign(HorizontalAlign.Center)
                .SetVerticalAlign(VerticalAlign.Bottom)
                .SetColor(Color.White);
            popup.AddComponent(new ScorePopup(points));
        }

        public override void OnAddedToEntity()
        {
            _text = Entity.GetComponent<TextComponent>();
        }

        public void Update()
        {
            _elapsed += Time.DeltaTime;
            Entity.Position += new Vector2(0, -RiseSpeed * Time.DeltaTime);

            var alpha = Mathf.Clamp01(1f - _elapsed / Duration);
            _text.SetColor(Color.White * alpha);

            if (_elapsed >= Duration)
                Entity.Destroy();
        }
    }
}
