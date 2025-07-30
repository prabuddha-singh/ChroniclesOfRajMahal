using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;


namespace PrabuddhaSingh.FinalCharachterController
{
    public class PlayerInputActions : MonoBehaviour, PlayerControls.IPlayerActionsMapActions
    {

        public bool AttackPressed { get; private set; }
        public bool GatherPressed { get; private set; }

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerState _playerState;

        void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
        }

        private void OnEnable()
        {

            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Cannot enable Player Controls");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.Enable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.SetCallbacks(this);
        }

        private void OnDisable()
        {

            if (PlayerInputManager.Instance?.PlayerControls == null)
            {
                Debug.LogError("Cannot diable Player Controls");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.Disable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.RemoveCallbacks(this);
        }

        public void SetGatherPressedFalse()
        {
            GatherPressed = false;
        }

        public void SetAttackPressedFalse()
        {
            AttackPressed = false;
        }

        void Update()
        {
            if (_playerLocomotionInput.MovementInput != Vector2.zero ||
                 _playerState.CurrentPlayerMovementState == PlayerMovementState.jumping ||
                  _playerState.CurrentPlayerMovementState == PlayerMovementState.falling)
            {
                GatherPressed = false;
            }
        }

        

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            AttackPressed = true;
        }

        public void OnGather(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            GatherPressed= true;
        }
    }

}

