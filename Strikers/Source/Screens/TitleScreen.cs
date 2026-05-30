using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace Strikers.Screens
{
    public class TitleScreen : GameScreen
    {
        private readonly Game1 _game;
        private Texture2D _background;
        private Texture2D _ship;
        private Texture2D _enemy;
        private bool _confirmHeld;
        private bool _moveHeld;
        private int _accent;

        public TitleScreen(Game1 game) : base(game)
        {
            _game = game;
            game.IsMouseVisible = true;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            _background = Content.Load<Texture2D>(Assets.Backgrounds.Tile);
            _ship = Content.Load<Texture2D>(Assets.Sprites.PlayerShip);
            _enemy = Content.Load<Texture2D>(Assets.Sprites.EnemyFighter);
            _game.Music.Play(Assets.Music.Title);
        }

        public override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            var pad = GamePad.GetState(PlayerIndex.One);

            if (pad.Buttons.Back == ButtonState.Pressed)
                Game.Exit();

            bool move = kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.Right) ||
                        kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.Down) ||
                        kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.D) ||
                        kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.S) ||
                        (pad.IsConnected && (
                            pad.DPad.Left == ButtonState.Pressed ||
                            pad.DPad.Right == ButtonState.Pressed ||
                            pad.DPad.Up == ButtonState.Pressed ||
                            pad.DPad.Down == ButtonState.Pressed));

            if (move && !_moveHeld)
            {
                _accent = (_accent + 1) % 3;
                _game.Sfx.Play(Assets.Sfx.MenuMove);
            }
            _moveHeld = move;

            bool confirm = kb.IsKeyDown(Keys.Enter) ||
                           kb.IsKeyDown(Keys.Space) ||
                           (pad.IsConnected && pad.Buttons.Start == ButtonState.Pressed);

            if (confirm && !_confirmHeld)
            {
                _game.Sfx.Play(Assets.Sfx.MenuSelect);
                ScreenManager.ReplaceScreen(new GameplayScreen(_game),
                    new FadeTransition(GraphicsDevice, Color.Black, 0.5f));
            }
            _confirmHeld = confirm;
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(8, 12, 18));

            var batch = _game.SpriteBatch;
            batch.Begin(samplerState: SamplerState.PointClamp);

            DrawBackground(batch);
            DrawTitle(batch, gameTime);

            batch.End();
        }

        private void DrawBackground(SpriteBatch batch)
        {
            batch.Draw(_background, new Rectangle(0, 0, VirtualResolution.Width, VirtualResolution.Height),
                new Color(60, 68, 86));
            batch.Draw(_enemy, new Rectangle(120, 160, 64, 64), Color.White * 0.75f);
            batch.Draw(_enemy, new Rectangle(410, 220, 64, 64), Color.White * 0.65f);
            batch.Draw(_ship, new Rectangle(VirtualResolution.Width / 2 - 36, 555, 72, 72), Color.White);
        }

        private void DrawTitle(SpriteBatch batch, GameTime gameTime)
        {
            var accent = _accent switch
            {
                1 => Color.Cyan,
                2 => Color.OrangeRed,
                _ => Color.Gold,
            };

            DrawCentered(batch, "STRIKERS", 235f, accent, 3f);
            DrawCentered(batch, "1945-STYLE ECS SHOOTER", 300f, Color.White * 0.8f, 0.85f);

            bool showPrompt = ((int)(gameTime.TotalGameTime.TotalSeconds * 2f) & 1) == 0;
            if (showPrompt)
                DrawCentered(batch, "PRESS ENTER / START", 470f, Color.White, 1.1f);
        }

        private void DrawCentered(SpriteBatch batch, string text, float centerY, Color color, float scale)
        {
            var size = _game.Font.MeasureString(text) * scale;
            var pos = new Vector2((VirtualResolution.Width - size.X) / 2f, centerY - size.Y / 2f);
            batch.DrawString(_game.Font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
