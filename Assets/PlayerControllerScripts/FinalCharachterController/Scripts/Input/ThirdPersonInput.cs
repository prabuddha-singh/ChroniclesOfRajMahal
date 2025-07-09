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
        [SerializeField] private float _cameraMinZoom =1f;
        [SerializeField] private float _cameraMaxZoom =5f;

         private Cinemachine3rdPersonFollow _thirdPersonFollow;

        void Awake()
        {
            _thirdPersonFollow = _virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        }

        private void OnEnable()
        {

            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Cannot enable Player Controls");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.SetCallbacks(this);
        }

        private void OnDisable()
        {

            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Cannot diable Player Controls");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
        }
        public void OnMouseScroll(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Vector2 scrollInput = context.ReadValue<Vector2>();
            ScrollInput = -1f * (scrollInput.normalized * _cameraZoomSpeed);

        }

        private void Update()
        {
            _thirdPersonFollow.CameraDistance = Mathf.Clamp(_thirdPersonFollow.CameraDistance + ScrollInput.y, _cameraMinZoom, _cameraMaxZoom);
        }

        private void LateUpdate(){
            ScrollInput = Vector2.zero;
        }
    }

}

