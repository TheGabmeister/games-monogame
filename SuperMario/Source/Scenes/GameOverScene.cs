using System;
using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class GameOverScene : Scene
    {
        public event Action Continue;

        public override void Initialize()
        {
            ClearColor = Color.Black;

            SetDesignResolution(
                Constants.ScreenWidth,
                Constants.ScreenHeight,
                SceneResolutionPolicy.ShowAllPixelPerfect);

            AddRenderer(new ScreenSpaceRenderer(0, RenderLayers.Hud));
        }

        public override void OnStart()
        {
            Audio.PlayMusic(Assets.Music.GameOver);

            var center = new Vector2(Constants.ScreenWidth / 2f, Constants.ScreenHeight / 2f);
            var entity = CreateEntity("game-over", center);
            entity.Scale = new Vector2(5);

            var text = entity.AddComponent(new TextComponent());
            text.SetText("Game Over");
            text.SetHorizontalAlign(HorizontalAlign.Center);
            text.SetVerticalAlign(VerticalAlign.Center);
            text.SetRenderLayer(RenderLayers.Hud);

            entity.AddComponent(new GameOverController(() => Continue?.Invoke()));
        }
    }
}
