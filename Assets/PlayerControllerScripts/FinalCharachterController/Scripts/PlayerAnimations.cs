using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController{
    public class PlayerAnimations : MonoBehaviour
  {
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed=0.02f ; 

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    private PlayerController _playerController;
    private PlayerInputActions _playerInputActions;
    

     // locomotion hashes 
    private static int inputHashX = Animator.StringToHash("InputX");
    private static int inputHashY = Animator.StringToHash("InputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int isIdlingHash = Animator.StringToHash("isIdling");

    private static readonly int verticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private static readonly int isInJumpAnimHash = Animator.StringToHash("isInJumpAnim");

    //action hashes 
   
    private static int isAttackingHash = Animator.StringToHash("isAttacking");
    private static int isGatheringHash = Animator.StringToHash("isGathering");
    private static int isPlayingActionHash = Animator.StringToHash("isPLayingAction");
    
    private static int isHitHash = Animator.StringToHash("isHit");
    private int[] actionHashes;
    

    // camera hashes 

    private static int rotationMismatchHash = Animator.StringToHash("RotationMismatch");
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");

    private float _sprintMaxBlendValue = 1.5f;
    private float _runMaxBlendValue =1f;
    private float _walkMaxBlendValue = 0.5f;

    private Vector3 _currentBlendInput = Vector3.zero;

        void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
            _playerController = GetComponent<PlayerController>();
            _playerInputActions = GetComponentInParent<PlayerInputActions>();

            actionHashes = new int[] { isGatheringHash , isAttackingHash , isHitHash};
        }

        void Update(){
            UpdateAnimationState();
            Debug.Log(_playerState.CurrentPlayerActionState);
        }

        private void UpdateAnimationState()
        {
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.sprinting;
            bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.jumping;
            bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.running;
            bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.falling;
            bool isGrounded = _playerController.IsGroundedExpose;
            float animVelocity = Mathf.Clamp(_playerController.VerticalVelocityExpose,5f,-5f);
            bool isPLayingAction = actionHashes.Any(hash => _animator.GetBool(hash));

            bool isRunBlendValue = isRunning || isJumping || isFalling;

            Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * _sprintMaxBlendValue :
                                 isRunBlendValue ? _playerLocomotionInput.MovementInput * _runMaxBlendValue :
                                 _playerLocomotionInput.MovementInput * _walkMaxBlendValue;

            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);
            


            _animator.SetFloat(inputHashX, _currentBlendInput.x);
            _animator.SetFloat(inputHashY, _currentBlendInput.y);
            _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
            _animator.SetBool(isGroundedHash, isGrounded);
            _animator.SetBool(isFallingHash, isFalling);
            _animator.SetBool(isJumpingHash, isJumping);
            _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
            _animator.SetBool(isIdlingHash, isIdling);
            _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);
            _animator.SetBool(isGatheringHash, _playerInputActions.GatherPressed);
            _animator.SetBool(isAttackingHash, _playerState.CurrentPlayerActionState == PlayerActionStates.Attacking);
            _animator.SetBool(isHitHash , _playerState.CurrentPlayerActionState == PlayerActionStates.Hit);
            _animator.SetBool(isPlayingActionHash, isPLayingAction);
            _animator.SetFloat(verticalVelocityHash, animVelocity);
            _animator.SetBool(isInJumpAnimHash, _playerController.IsInJumpAnim);
            
        }

    }

}
