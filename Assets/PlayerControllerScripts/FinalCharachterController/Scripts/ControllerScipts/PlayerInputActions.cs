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

        public bool GatherPressed { get; private set; }

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerState _playerState;
        private PlayerController _playerController;
        private PlayerCombat _playerCombat;

        void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
            _playerController = GetComponent<PlayerController>();
            _playerCombat = GetComponent<PlayerCombat>();
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
                Debug.LogError("Cannot disable Player Controls");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.Disable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionsMap.RemoveCallbacks(this);
        }

        public void SetGatherPressedFalse()
        {
            GatherPressed = false;
            _playerState.ClearPlayerActionState();
        }

        void Update()
        {

            if (_playerState.IsPlayingAction()) return ;
            else if (GatherPressed)
            {
                _playerState.SetPlayerActionState(PlayerActionStates.Gathering);
                _playerController.ResetLateralVelocity();
                GatherPressed = false;
            }
        }
        public void OnAttack(InputAction.CallbackContext context)
        {
            
            if (!context.performed)
            {
                return;
            }
 
            _playerCombat.HandleActionInput();
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

