using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Asteroids.Input
{
    public sealed class InputState
    {
        private KeyboardState _currentKeyboard;
        private KeyboardState _previousKeyboard;
        private GamePadState _currentGamePad;
        private GamePadState _previousGamePad;

        public void Update()
        {
            _previousKeyboard = _currentKeyboard;
            _previousGamePad = _currentGamePad;

            _currentKeyboard = Keyboard.GetState();
            _currentGamePad = GamePad.GetState(PlayerIndex.One);
        }

        public bool IsExitPressed =>
            IsNewKeyPress(Keys.Escape) ||
            IsNewButtonPress(Buttons.Back);

        public bool IsPausePressed =>
            IsNewKeyPress(Keys.Escape) ||
            IsNewButtonPress(Buttons.Start);

        public bool IsConfirmPressed =>
            IsNewKeyPress(Keys.Enter) ||
            IsNewKeyPress(Keys.Space) ||
            IsNewButtonPress(Buttons.A) ||
            IsNewButtonPress(Buttons.Start);

        public bool IsFirePressed =>
            IsKeyDown(Keys.Space) ||
            _currentGamePad.IsButtonDown(Buttons.A) ||
            _currentGamePad.Triggers.Right > 0.35f;

        public bool IsFireNewPress =>
            IsNewKeyPress(Keys.Space) ||
            IsNewButtonPress(Buttons.A) ||
            (_currentGamePad.Triggers.Right > 0.35f && _previousGamePad.Triggers.Right <= 0.35f);

        public bool IsThrustDown =>
            IsKeyDown(Keys.Up) ||
            IsKeyDown(Keys.W) ||
            _currentGamePad.IsButtonDown(Buttons.X) ||
            _currentGamePad.Triggers.Left > 0.35f;

        public float RotationAxis
        {
            get
            {
                float keyboardAxis = 0f;

                if (IsKeyDown(Keys.Left) || IsKeyDown(Keys.A))
                    keyboardAxis -= 1f;

                if (IsKeyDown(Keys.Right) || IsKeyDown(Keys.D))
                    keyboardAxis += 1f;

                float gamePadAxis = 0f;

                if (_currentGamePad.IsConnected)
                {
                    gamePadAxis = _currentGamePad.ThumbSticks.Left.X;

                    if (_currentGamePad.DPad.Left == ButtonState.Pressed)
                        gamePadAxis -= 1f;

                    if (_currentGamePad.DPad.Right == ButtonState.Pressed)
                        gamePadAxis += 1f;
                }

                float axis = keyboardAxis != 0f ? keyboardAxis : gamePadAxis;

                if (axis < -1f)
                    return -1f;

                if (axis > 1f)
                    return 1f;

                return axis;
            }
        }

        private bool IsKeyDown(Keys key) => _currentKeyboard.IsKeyDown(key);

        private bool IsNewKeyPress(Keys key) =>
            _currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

        private bool IsNewButtonPress(Buttons button) =>
            _currentGamePad.IsButtonDown(button) && _previousGamePad.IsButtonUp(button);
    }
}
