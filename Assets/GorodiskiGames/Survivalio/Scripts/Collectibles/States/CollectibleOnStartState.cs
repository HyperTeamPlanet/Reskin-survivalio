using Game.UI;
using Injection;

namespace Game.Collectible.States
{
    public sealed class CollectibleOnStartState : CollectibleState
    {
        [Inject] private GameView _gameView;

        public override void Initialize()
        {
            _gameView.Joystick.ON_FIRST_TOUCH += OnFirstTouch;
        }

        public override void Dispose()
        {
            _gameView.Joystick.ON_FIRST_TOUCH -= OnFirstTouch;
        }

        private void OnFirstTouch()
        {
            _collectible.Idle();
        }
    }
}

