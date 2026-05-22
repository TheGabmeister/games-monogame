using Microsoft.Xna.Framework;
using Nez;
using Nez.Systems;
using Nez.Tweens;

namespace SpaceInvaders
{
    public static class ScorePopup
    {
        public static void Spawn(Scene scene, Vector2 position, int points, Color color, float scale = 2f)
        {
            var font = Graphics.Instance.BitmapFont;
            var entity = scene.CreateEntity("score-popup", position);
            var text = entity.AddComponent(new TextComponent(font, $"+{points}", Vector2.Zero, color));
            text.SetHorizontalAlign(HorizontalAlign.Center);
            entity.Transform.SetScale(scale);

            entity.TweenPositionTo(position + new Vector2(0, -40), 0.8f)
                .SetEaseType(EaseType.QuadOut)
                .Start();

            text.TweenColorTo(Color.Transparent, 0.8f)
                .SetEaseType(EaseType.QuadIn)
                .SetCompletionHandler(_ => { if (!entity.IsDestroyed) entity.Destroy(); })
                .Start();
        }
    }
}
