using Microsoft.Xna.Framework;
using Nez;

namespace Template
{
    public class Game1 : Core
    {
        public Game1()
            : base(width: 1280, height: 720, isFullScreen: false, windowTitle: "Template")
        {
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            var scene = Scene.CreateWithDefaultRenderer(Color.CornflowerBlue);
            scene.SetDesignResolution(1280, 720, Scene.SceneResolutionPolicy.ShowAllPixelPerfect);

            var box = scene.CreateEntity("box", new Vector2(640, 360));
            box.AddComponent(new PrototypeSpriteRenderer(96, 96))
                .SetColor(Color.White);

            Scene = scene;
        }
    }
}
