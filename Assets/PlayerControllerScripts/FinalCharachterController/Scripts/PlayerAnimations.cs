using System.Collections;
using System.Collections.Generic;
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

    private static int inputHashX = Animator.StringToHash("InputX");
    private static int inputHashY = Animator.StringToHash("InputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int rotationMismatchHash = Animator.StringToHash("RotationMismatch");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");

    private float _sprintMaxBlendValue = 1.5f;
    private float _runMaxBlendValue =1f;
    private float _walkMaxBlendValue = 0.5f;

    private Vector3 _currentBlendInput = Vector3.zero;

        void Awake()
        {
            _playerLocomotionInput=GetComponent<PlayerLocomotionInput>();
            _playerState=GetComponent<PlayerState>();
            _playerController=GetComponent<PlayerController>();
        }

        void Update(){
            UpdateAnimationState();
        }

        private void UpdateAnimationState(){
           bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.sprinting;
           bool isIdling =  _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
           bool isJumping =  _playerState.CurrentPlayerMovementState == PlayerMovementState.jumping;
           bool isRunning = _playerState.CurrentPlayerMovementState==PlayerMovementState.running;
           bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.falling;
           bool isGrounded = _playerState.InGroundedState();

           bool isRunBlendValue = isRunning || isJumping || isFalling ; 

            Vector2 inputTarget= isSprinting?  _playerLocomotionInput.MovementInput * _sprintMaxBlendValue : 
                                 isRunBlendValue ? _playerLocomotionInput.MovementInput *_runMaxBlendValue :
                                 _playerLocomotionInput.MovementInput * _walkMaxBlendValue;

            _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget , locomotionBlendSpeed * Time.deltaTime);

            
            
            _animator.SetFloat(inputHashX , _currentBlendInput.x);
            _animator.SetFloat(inputHashY , _currentBlendInput.y);
            _animator.SetFloat(inputMagnitudeHash , _currentBlendInput.magnitude);
            _animator.SetBool(isGroundedHash , isGrounded) ;
            _animator.SetBool(isFallingHash , isFalling) ;
            _animator.SetBool(isJumpingHash , isJumping) ;
            _animator.SetFloat(rotationMismatchHash , _playerController.RotationMismatch);
            _animator.SetBool(isIdlingHash , isIdling) ;
            _animator.SetBool(isRotatingToTargetHash , _playerController.IsRotatingToTarget);
            
        }

    }

}
