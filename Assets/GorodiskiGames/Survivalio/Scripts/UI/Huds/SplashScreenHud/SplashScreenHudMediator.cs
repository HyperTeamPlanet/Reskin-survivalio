using System;
using Game.Config;
using Game.Core;
using Game.Core.UI;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class SplashScreenHudMediator : Mediator<SplashScreenHudView>
    {
        [Inject] private GameConfig _config;
        [Inject] private Timer _timer;
        [Inject] private HudManager _hudManager;

        private float _duration;
        private float _elapsed;

        private bool _calculateDuration;

        public SplashScreenHudMediator(bool calculateDuration)
        {
            _calculateDuration = calculateDuration;
        }

        protected override void Show()
        {
            var deviceID = SystemInfo.deviceUniqueIdentifier;
            _view.DeviceIDText.text = deviceID;

          //  var isTestDevice = TestDeviceIDs.IsTestDevice();
           /* if (isTestDevice)
                _view.DeviceIDText.color = Color.red;*/

            _duration = _config.SplashScreenDurationMobile;

#if UNITY_EDITOR
            _duration = _config.SplashScreenDurationEditor;
#endif

            _elapsed = 0f;
            UpdateBar();

            _view.FillBarHolder.SetActive(_calculateDuration);

            if (!_calculateDuration)
                return;
           
            _timer.TICK += OnTICK;
        }

        protected override void Hide()
        {
            _timer.TICK -= OnTICK;
        }

        private void OnTICK()
        {
            _elapsed += Time.deltaTime;
            UpdateBar();

            if (_elapsed < _duration)
                return;
            
            _hudManager.HideAdditional<SplashScreenHudMediator>();

            ON_HIDE?.Invoke();
        }

        private void UpdateBar()
        {
            float value = _elapsed / _duration;
            _view.FillBarImage.fillAmount = value;
        }
    }
}