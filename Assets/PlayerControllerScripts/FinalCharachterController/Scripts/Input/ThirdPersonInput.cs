using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class ThirdPersonInput : MonoBehaviour, PlayerControls.IThirdPersonMapActions
    {

        public Vector2 ScrollInput { get; private set; }

        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private float _cameraZoomSpeed = 0.1f;
        [SerializeField] private float _cameraMinZoom = 1f;
        [SerializeField] private float _cameraMaxZoom = 5f;

        private Cinemachine3rdPersonFollow _thirdPersonFollow;

        void Awake()
        {
            _thirdPersonFollow = _virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        }

        private void OnEnable()
        {
            Debug.Log($"ThirdPersonInput ENABLED | " + $"ID: {GetInstanceID()} | " + $"Scene: {gameObject.scene.name}");

            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.SetCallbacks(this);
        }

        private void OnDisable()
        {

            Debug.Log($"ThirdPersonInput DISABLED | " + $"ID: {GetInstanceID()} | " + $"Scene: {gameObject.scene.name}");

            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                return;
            }

            Debug.Log($"ThirdPersonMap being disabled | " + $"ID: {GetInstanceID()} | " + $"Map enabled: {PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.enabled}");

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
        }
        public void OnMouseScroll(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Vector2 scrollInput = context.ReadValue<Vector2>();
            ScrollInput = -scrollInput * _cameraZoomSpeed;

        }

        private void Update()
        {
            if (Mathf.Abs(ScrollInput.y) > 0.001f)
            {
                _thirdPersonFollow.CameraDistance = Mathf.Clamp(_thirdPersonFollow.CameraDistance + ScrollInput.y, _cameraMinZoom, _cameraMaxZoom);
            }
            ScrollInput = Vector2.zero;
        }

    }

}
