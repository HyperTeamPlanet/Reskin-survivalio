using System.Collections.Generic;
using Game.Controls;
using Game.Player;
using Game.UI.Hud;
using Game.Weapon;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class GameView : MonoBehaviour
    {
        [SerializeField] private bool _isRecordTrailer;
        [SerializeField] private AspectRatioFitter _aspectRatio;
        public BaseHud[] Huds;
        public Canvas Canvas;
        public Joystick Joystick;
        public PlayerView PlayerView;
        public WeaponView WeaponView;
        public Transform BarsHolder;
        public CameraController Camera;
        public GameObject MenuLight;
        public GameObject LevelLight;

        public IEnumerable<IHud> AllHuds()
        {
            foreach (var hud in Huds)
            {
                yield return hud;
            }
        }

        private void Awake()
        {
            SetAspectRatioFitter();
        }

        private void SetAspectRatioFitter()
        {
            if (!_isRecordTrailer)
                return;

            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = false;

            var mode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            var ratio = 16f / 9f;

            _aspectRatio.aspectMode = mode;
            _aspectRatio.aspectRatio = ratio;
            _aspectRatio.enabled = true;
        }
    }
}

