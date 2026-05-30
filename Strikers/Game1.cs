using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace Strikers
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly ScreenManager _screenManager;

        // Resources that outlive a single run, loaded once and reused when the stage is
        // restarted. Textures/sounds are cached by the ContentManager anyway.
        public SpriteBatch SpriteBatch { get; private set; }
        public AnimationLibrary Animations { get; private set; }
        public SfxManager Sfx { get; private set; }
        public MusicManager Music { get; private set; }
        public SpriteFont Font { get; private set; }
        public Texture2D LifeIcon { get; private set; }
        public Texture2D BombIcon { get; private set; }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = VirtualResolution.Width,
                PreferredBackBufferHeight = VirtualResolution.Height,
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _screenManager = Components.Add<ScreenManager>();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Animations = new AnimationLibrary(Content);
            Animations.Load();
            Sfx = new SfxManager(Content);
            Music = new MusicManager(Content);
            Font = Content.Load<SpriteFont>(Assets.Fonts.Main);
            LifeIcon = Content.Load<Texture2D>(Assets.Sprites.HudLifeIcon);
            BombIcon = Content.Load<Texture2D>(Assets.Sprites.HudBombIcon);

            _screenManager.ShowScreen(new Screens.TitleScreen(this),
                new FadeTransition(GraphicsDevice, Color.Black, 0.5f));
        }
    }
}
