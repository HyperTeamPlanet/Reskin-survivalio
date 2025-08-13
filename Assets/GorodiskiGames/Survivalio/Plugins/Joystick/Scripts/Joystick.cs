using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Controls
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public event Action ON_FIRST_TOUCH;

        private const float _maxRadius = 125f;

        [SerializeField] private GameObject _background, _handle;

        [HideInInspector] public bool IsTouched;
        [HideInInspector] public float Horizontal, Vertical;

        private Vector2 _touchPosition;
        private Touch _oneTouch;
        private bool _joystickVisibility;

        private void Start()
        {
            _joystickVisibility = false;

            SpritesVisibility(false);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            ON_FIRST_TOUCH?.Invoke();

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {

#if !UNITY_EDITOR
            if (eventData.IsPointerMoving() && eventData.pointerId == 0)
        {
            _oneTouch = Input.GetTouch(0);
            _touchPosition = _oneTouch.position;

            if (!IsTouched)
            {
                IsTouched = true;

                SpritesVisibility(true);

                _background.transform.position = _touchPosition;
                _handle.transform.position = _touchPosition;
            }
            Move();
        }
#endif

        }

        private void Update()
        {

#if UNITY_EDITOR || UNITY_WEBGL
            Horizontal = Input.GetAxis("Horizontal");
            Vertical = Input.GetAxis("Vertical");

            if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            {
                IsTouched = true;
                ON_FIRST_TOUCH?.Invoke();
            }
            else
                IsTouched = false;
#endif

        }

        private void OnDisable()
        {
            OnPointerUp(null);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            IsTouched = false;

            SpritesVisibility(false);
        }

        private void Move()
        {
            _handle.transform.position = _touchPosition;
            _handle.transform.localPosition = Vector2.ClampMagnitude(_handle.transform.localPosition, _maxRadius);

            Horizontal = _handle.transform.localPosition.x / _maxRadius;
            Vertical = _handle.transform.localPosition.y / _maxRadius;
        }

        private void SpritesVisibility(bool value)
        {
            if (_joystickVisibility == false)
                value = false;

            _background.SetActive(value);
            _handle.SetActive(value);
        }

        public void JoystickVisibility(bool value)
        {
            _joystickVisibility = value;
        }

        public void SetEnable(bool value)
        {
            enabled = value;
        }
    }
}



