using Microsoft.Xna.Framework;
using Nez;

namespace Template.Scenes
{
    public class MainScene : Scene
    {
        public override void Initialize()
        {
            ClearColor = Color.CornflowerBlue;

            SetDesignResolution(
                Constants.ScreenWidth,
                Constants.ScreenHeight,
                SceneResolutionPolicy.ShowAllPixelPerfect);

            AddRenderer(new DefaultRenderer());

            var center = new Vector2(Constants.ScreenWidth / 2f, Constants.ScreenHeight / 2f);
            var box = CreateEntity("box", center);
            box.AddComponent(new PrototypeSpriteRenderer(96, 96))
                .SetColor(Color.White);
        }
    }
}
