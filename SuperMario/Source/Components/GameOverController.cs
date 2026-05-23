using System;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace SuperMario
{
    public class GameOverController : Component, IUpdatable
    {
        readonly Action _onContinue;
        VirtualButton _continueButton;

        public GameOverController(Action onContinue)
        {
            _onContinue = onContinue;
        }

        public override void OnAddedToEntity()
        {
            _continueButton = new VirtualButton();
            _continueButton.AddKeyboardKey(Keys.Enter);
            _continueButton.AddKeyboardKey(Keys.Space);
            _continueButton.AddGamePadButton(0, Buttons.Start);
        }

        public override void OnRemovedFromEntity()
        {
            _continueButton.Deregister();
        }

        public void Update()
        {
            if (_continueButton.IsPressed)
                _onContinue?.Invoke();
        }
    }
}
