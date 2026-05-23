using System;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace SuperMario
{
    public class MainMenuController : Component, IUpdatable
    {
        readonly Action _onStart;
        VirtualButton _startButton;

        public MainMenuController(Action onStart)
        {
            _onStart = onStart;
        }

        public override void OnAddedToEntity()
        {
            _startButton = new VirtualButton();
            _startButton.AddKeyboardKey(Keys.Enter);
            _startButton.AddKeyboardKey(Keys.Space);
            _startButton.AddGamePadButton(0, Buttons.Start);
        }

        public override void OnRemovedFromEntity()
        {
            _startButton.Deregister();
        }

        public void Update()
        {
            if (_startButton.IsPressed)
                _onStart?.Invoke();
        }
    }
}
