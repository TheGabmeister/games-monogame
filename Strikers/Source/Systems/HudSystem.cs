using System.Collections.Generic;
using Strikers.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace Strikers.Systems
{
    // Draws the arcade HUD on top of the rendered scene: score (from GameState), and the
    // player's weapon power, spare lives, and bomb stock (from the Player component). When
    // the run ends it draws the centered STAGE CLEAR / GAME OVER banner plus a restart
    // prompt (see PLAN.md §5). Its own SpriteBatch.Begin runs after RenderSystem's, so the
    // HUD always sits in front; it doesn't need depth sorting.
    public class HudSystem : EntityDrawSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _lifeIcon;
        private readonly Texture2D _bombIcon;

        // Set by Game1 after the World is built (factory-wiring note in AGENTS.md).
        public GameState State;

        private ComponentMapper<Player> _playerMapper;

        private const float Pad = 12f;
        private const int IconSize = 24;
        private const int IconStep = 28;

        public HudSystem(SpriteBatch spriteBatch, SpriteFont font, Texture2D lifeIcon, Texture2D bombIcon)
            : base(Aspect.All(typeof(Player)))
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _lifeIcon = lifeIcon;
            _bombIcon = bombIcon;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _playerMapper = mapperService.GetMapper<Player>();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            int score = State?.Score ?? 0;
            _spriteBatch.DrawString(_font, "SCORE", new Vector2(Pad, Pad), Color.White);
            _spriteBatch.DrawString(_font, score.ToString("D7"), new Vector2(Pad, Pad + 24f), Color.White);

            // Pull the (single) player's stats, if it still exists.
            Player player = null;
            foreach (var id in ActiveEntities)
            {
                player = _playerMapper.Get(id);
                break;
            }

            if (player != null)
            {
                _spriteBatch.DrawString(_font, $"POWER {player.WeaponLevel}",
                    new Vector2(Pad, Pad + 48f), Color.Cyan);

                float rightX = VirtualResolution.Width - Pad;
                DrawIconRow(_lifeIcon, player.Lives, rightX, Pad);
                DrawIconRow(_bombIcon, player.Bombs, rightX, Pad + IconStep);
            }

            DrawBanner();

            _spriteBatch.End();
        }

        // Draws `count` icons in a row ending flush against rightX.
        private void DrawIconRow(Texture2D icon, int count, float rightX, float y)
        {
            for (int i = 0; i < count; i++)
            {
                float x = rightX - (count - i) * IconStep;
                _spriteBatch.Draw(icon, new Rectangle((int)x, (int)y, IconSize, IconSize), Color.White);
            }
        }

        private void DrawBanner()
        {
            if (State == null || State.Phase == GamePhase.Playing)
                return;

            string title = State.Phase == GamePhase.StageClear ? "STAGE CLEAR" : "GAME OVER";
            var titleColor = State.Phase == GamePhase.StageClear ? Color.Gold : Color.OrangeRed;

            DrawCentered(title, VirtualResolution.Height / 2f - 30f, titleColor, 2f);
            DrawCentered("PRESS ENTER", VirtualResolution.Height / 2f + 30f, Color.White, 1f);
        }

        private void DrawCentered(string text, float centerY, Color color, float scale)
        {
            var size = _font.MeasureString(text) * scale;
            var pos = new Vector2((VirtualResolution.Width - size.X) / 2f, centerY - size.Y / 2f);
            _spriteBatch.DrawString(_font, text, pos, color, 0f, Vector2.Zero, scale,
                SpriteEffects.None, 0f);
        }
    }
}
