using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class SplashScreenHudView : BaseHud
    {
        [SerializeField] private GameObject _fillBarHolder;
        [SerializeField] private Image _fillBarImage;
        [SerializeField] private TMP_Text _deviceIDText;

        public GameObject FillBarHolder => _fillBarHolder;
        public Image FillBarImage => _fillBarImage;
        public TMP_Text DeviceIDText => _deviceIDText;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {
            
        }
    }
}