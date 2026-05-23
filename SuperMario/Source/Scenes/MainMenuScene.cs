using System;
using Microsoft.Xna.Framework;
using Nez;

namespace SuperMario
{
    public class MainMenuScene : Scene
    {
        public event Action StartPressed;

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
            Core.GetGlobalManager<MusicManager>().Play(Assets.Music.MainMenu);

            var center = new Vector2(Constants.ScreenWidth / 2f, Constants.ScreenHeight / 2f);
            var titleEntity = CreateEntity("title", new Vector2(center.X, center.Y - 96));
            titleEntity.Scale = new Vector2(5);

            var title = titleEntity.AddComponent(new TextComponent());
            title.SetText("Super Mario Bros");
            title.SetHorizontalAlign(HorizontalAlign.Center);
            title.SetVerticalAlign(VerticalAlign.Center);
            title.SetRenderLayer(RenderLayers.Hud);

            var entity = CreateEntity("press-start", center);
            entity.Scale = new Vector2(5);

            var text = entity.AddComponent(new TextComponent());
            text.SetText("Press Start");
            text.SetHorizontalAlign(HorizontalAlign.Center);
            text.SetVerticalAlign(VerticalAlign.Center);
            text.SetRenderLayer(RenderLayers.Hud);

            entity.AddComponent(new MainMenuController(() => StartPressed?.Invoke()));
        }
    }
}
