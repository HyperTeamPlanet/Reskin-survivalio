using Game.Core.UI;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public sealed class SettingsHudMediator : Mediator<SettingsHudView>
    {
        [Inject] private HudManager _hudManager;

        protected override void Show()
        {
            var isDebugBuild = GameConstants.IsDebugBuild();
            _view.CheatPanelButton.gameObject.SetActive(isDebugBuild);

            _view.CheatPanelButton.onClick.AddListener(OnCheatPanelButtonClick);
            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
        }

        protected override void Hide()
        {
            _view.CheatPanelButton.onClick.RemoveListener(OnCheatPanelButtonClick);
            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        private void OnCloseButtonClick()
        {
            _hudManager.HideSingle();
        }

        private void OnCheatPanelButtonClick()
        {
            _hudManager.ShowAdditional<CheatPanelHudMediator>();
        }
    }
}

