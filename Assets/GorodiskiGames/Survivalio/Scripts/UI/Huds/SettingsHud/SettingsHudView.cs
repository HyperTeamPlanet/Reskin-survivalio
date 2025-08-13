using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class SettingsHudView : BaseHud
    {
        [SerializeField] private Button _cheatPanelButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _restorePurchasesButton;

        public Button CheatPanelButton => _cheatPanelButton;
        public Button CloseButton => _closeButton;
        public Button RestorePurchasesButton => _restorePurchasesButton;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}


