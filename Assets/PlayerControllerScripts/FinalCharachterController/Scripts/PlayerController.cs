using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
using System;

namespace PrabuddhaSingh.FinalCharachterController{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour , IKnockbackable 
{
    [Header("Componenets")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;
    public float RotationMismatch   {get; private set;} =0f;
    public bool IsRotatingToTarget {get; private set;} = false;

    [Header("Base Movement")]
    public float acceleration=0.25f;
    public float speed=4f;
    public float walkSpeed= 3f;
    public float walkAcceleration= 0.3f;
    public float sprintAcceleration = 0.5f;
    public float inAirAcceleration = 0.15f;
    public float sprintSpeed = 7f;
    public float drag=0.1f;
    public float movingThreshold= 0.01f;
    public float gravity =-25f;
    public float jumpSpeed=7f;
    public float terminalVelocity =-50f;
    public bool IsGroundedExpose => _isGrounded;

    public float VerticalVelocityExpose => _verticalVelocity;
    
    
   
   [Header("Animations")]
   public float modelRotationSpeed = 10f;
   public float rotationTime = 0.25f;

    [Header("Camera Settings")]
    public float lookSenseH=0.1f;
    public float lookSenseV=0.1f;
    public float loolLimitV=89f;

    [Header("environment details")]
    [SerializeField] private LayerMask _groundLayers;

    private PlayerLocomotionInput _playerLocomotionInput;
    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;
    private float _rotationTimer = 0f;
    private float _verticalVelocity = 0f;
    private bool _isRotatingclockWise = false;
    private float _jumpGraceTimer ;
    private const float JumpGraceTime = 0.12f;
    private float _stepOffSet ;
    private bool _isGrounded;
    private float _groundedBufferTimer;
    private Vector3 _chachedLateralVelocity ; 
    private bool _ledgeAssistUsed;
    private float groundedBufferTime = 0.08f;
    private const float GroundStickForce = -2f;
    private Vector3 _knockbackVelocity;
    private float _knockbackTimer;
    private bool _isKnockbackActive;
    
    [SerializeField] private float ledgeAssistWindow = 0.25f;
    private float ledgeAssistTimer ; 
    
    private PlayerMovementState _lastMovementState = PlayerMovementState.falling;

    private float _jumpAnimTimer ;
    public bool IsInJumpAnim => _jumpAnimTimer > 0f;

    private PlayerState _playerState;

        void Awake()
        {
            _playerLocomotionInput  = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
            _stepOffSet = _characterController.stepOffset;
            _chachedLateralVelocity = Vector3.zero;
        }

        private void Update(){
            HandleKnockback();             // first handle knockback if any otherwise procees as follows 
            HandleVerticalMovement();      // first handle vertical movememt 
            if(_jumpAnimTimer >0f)
                _jumpAnimTimer -= Time.deltaTime;
            if(ledgeAssistTimer >0f)
                ledgeAssistTimer -= Time.deltaTime;
            HandleLateralMovement();          // then handle lateral movement 
            UpdateMovementState();            // prioritize updating movement state                                   
        }

        private void UpdateMovementState(){
            bool hasMoveInput = _playerLocomotionInput.MovementInput.sqrMagnitude > 0.01f;
            bool isGrounded = _isGrounded;
            if (isGrounded)
            {
                if (!hasMoveInput)
                {
                    _playerState.SetPlayerMovementState(PlayerMovementState.Idling);
                }
                else
                {
                    bool canRun = CanRun();
                    bool WantsSprint = _playerLocomotionInput.SprintToggledOn;
                    bool WantsWalk = _playerLocomotionInput.WalkToggledOn;

                    if(WantsSprint && canRun)
                    {
                        _playerState.SetPlayerMovementState(PlayerMovementState.sprinting);
                    }

                    else if(WantsWalk)
                    {
                        _playerState.SetPlayerMovementState(PlayerMovementState.walking);
                    }

                    else
                    {
                        _playerState.SetPlayerMovementState(PlayerMovementState.running); 
                    }

                }
            }

            if(!isGrounded){
                if(_verticalVelocity > 0f)
                {
                    _playerState.SetPlayerMovementState(PlayerMovementState.jumping);
                }
                else
                {
                    _playerState.SetPlayerMovementState(PlayerMovementState.falling);
                }            
            }
        }


        private void HandleKnockback()
        {
            if(!_isKnockbackActive)
            return;

            _knockbackTimer -= Time.deltaTime;

            if(_knockbackTimer <= 0f)
            {
                _isKnockbackActive = false;
                _knockbackVelocity = Vector3.zero;
                return;
            }

            _characterController.Move(_knockbackVelocity * Time.deltaTime);
        }

        private void HandleVerticalMovement(){
            if (_jumpGraceTimer > 0f)
                 _jumpGraceTimer -= Time.deltaTime;

            bool rawGrounded = _jumpGraceTimer <= 0f && IsGrounded(out _, out _);

            if (rawGrounded)
            {
                _groundedBufferTimer = groundedBufferTime;
            }
            else
            {
                _groundedBufferTimer -= Time.deltaTime;
            }

            _isGrounded = _groundedBufferTimer > 0f;

            

            if (_isGrounded){

                _ledgeAssistUsed= false;

                if (!_playerLocomotionInput.JumpPressed)
                {
                    _verticalVelocity = GroundStickForce;
                }
                if (_playerLocomotionInput.JumpPressed){
                _verticalVelocity = jumpSpeed;
                _jumpGraceTimer = JumpGraceTime;
                _jumpAnimTimer = 0.35f;

                ledgeAssistTimer = ledgeAssistWindow;
                }
           }
           else{
            _verticalVelocity += gravity * Time.deltaTime;

            if (_verticalVelocity < terminalVelocity)
                _verticalVelocity = terminalVelocity;
            }


        }
        private void HandleLateralMovement(){


            if(_isKnockbackActive)
                return;

            if (_playerState.BlocksMovement())
            {
                Vector3 velocity = new Vector3(0f, _verticalVelocity, 0f);
                _characterController.Move(velocity * Time.deltaTime);
                return;
            }

             bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.sprinting;
             bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.walking;
             bool isGroundedValue = _isGrounded;

            float lateralAcceleration = isGroundedValue?(isWalking ? walkAcceleration:
                                        isSprinting ? sprintAcceleration :
                                        acceleration)
                                        : inAirAcceleration;
                
            float maxSpeed = isWalking? walkSpeed:
                             isSprinting ?sprintSpeed :
                             speed;

            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;

            Vector3 movementDirection =
            cameraRightXZ * _playerLocomotionInput.MovementInput.x +
            cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

            Vector3 lateralVelocity = _chachedLateralVelocity;

            if (isGroundedValue)
            {
                lateralVelocity += movementDirection * lateralAcceleration * Time.deltaTime;

                if(lateralVelocity.magnitude > 0f)
                {
                    Vector3 dragVector = lateralVelocity.normalized * drag * Time.deltaTime;
                   lateralVelocity = (lateralVelocity.magnitude > dragVector.magnitude) ? lateralVelocity - dragVector : Vector3.zero;
                }

                lateralVelocity = Vector3.ClampMagnitude(lateralVelocity, maxSpeed);
            }

            else
            {
                Vector3 targetAirVelocity = movementDirection * maxSpeed;
                lateralVelocity = Vector3.Lerp(lateralVelocity,targetAirVelocity, lateralAcceleration * Time.deltaTime);
            }

            _chachedLateralVelocity = lateralVelocity;

             
          Vector3 finalVelocity = new Vector3(lateralVelocity.x,_verticalVelocity,lateralVelocity.z);

          
            ApplyLedgeAssist();
           _characterController.Move(finalVelocity * Time.deltaTime);
       }

        private void ApplyLedgeAssist()
        {
            if(_isGrounded || ledgeAssistTimer<=0f || _ledgeAssistUsed || _isKnockbackActive)
                return;
            
            
            float capsuleRadius = _characterController.radius *0.95f;
            float capsuleHeight = _characterController.height;

            Vector3 bottom = transform.position + _characterController.center + Vector3.up * capsuleRadius;
            Vector3 top = bottom + Vector3.up * (capsuleHeight - capsuleRadius *2f);

            Vector3 forward = transform.forward;

            if(!Physics.CapsuleCast(bottom , top , capsuleRadius , forward , out RaycastHit wallHit , capsuleRadius +0.05f, _groundLayers , QueryTriggerInteraction.Ignore))
                return;
            
            Vector3 stepUpStart = wallHit.point + Vector3.up * capsuleHeight * 0.4f;

            bool blockedAbove = Physics.Raycast(
              stepUpStart,
              Vector3.up,
              out _,
              0.4f,
              _groundLayers,
              QueryTriggerInteraction.Ignore
            );

            if (blockedAbove) return;

            _characterController.Move(Vector3.up * 0.35f);
            _ledgeAssistUsed = true;
        }

        public void ApplyKnockback(HitInfo hitInfo)
        {
            hitInfo.Direction.y = 0f;
            hitInfo.Direction.Normalize();

            _knockbackVelocity = hitInfo.Direction * hitInfo.Force;
            _knockbackTimer = hitInfo.Duration;
            _isKnockbackActive = true;

            ResetLateralVelocity();
            _playerState.SetPlayerActionState(PlayerActionStates.Hit);
        }

        private bool IsGrounded(out Vector3 groundNormal, out float slopeAngle){

            groundNormal = Vector3.up;
            slopeAngle = 0f;

            bool grounded = PlayerControlUtils.CheckGrounded(_characterController , out groundNormal);

           if(!grounded) return false;

           slopeAngle = Vector3.Angle(groundNormal , Vector3.up);

           if(slopeAngle <= _characterController.slopeLimit){
            return true;
           }

           if(_verticalVelocity <=0f){
            return true;
           }

           return false;
      }
        private void LateUpdate()
        {
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation(){
            _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -loolLimitV , loolLimitV);

            _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;

            float rotationTolerence = 90f;

            bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            IsRotatingToTarget = _rotationTimer >0;
            // agar RotationMismatch tolerence(90) se zada hai toh YA PHIR RotateToTarget active hai (timer zero se zada hai )toh ROTATE KARO
            // kabhi kabhi bhagte samay bhi mudna hoga to isliye ROTATE if you are NOT IDLING 

            if(!isIdling){
              RotatePlayerToTarget();
            }   

            else if(!isIdling || Mathf.Abs(RotationMismatch) >rotationTolerence || IsRotatingToTarget){
              UpdateRotation(rotationTolerence);
            }



            _playerCamera.transform.rotation=Quaternion.Euler(_cameraRotation.y , _cameraRotation.x , 0f);

           // sign varibale tells wether the camera is looking to left (1) , right (-1) , ahead (0) [boleto jhakas]
           // we multiply the calculated camForwardProjectXZ to the sign var to get both direction of the angle difference and magnitude
           // is value ko rotationMismatch me store kiya , agar mismatch positive = left , negative = right , zero = ahaed
            Vector3 camForwardProjectXZ = new Vector3(_playerCamera.transform.forward.x , 0f , _playerCamera.transform.forward.z ).normalized;
            Vector3 crossProd = Vector3.Cross(transform.forward, camForwardProjectXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProd , transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward ,camForwardProjectXZ);

        }

        private void UpdateRotation(float rotationTolerence){

            if(Mathf.Abs(RotationMismatch) > rotationTolerence){
                    _rotationTimer = rotationTime ; 
                    _isRotatingclockWise = RotationMismatch > rotationTolerence ; 
                }
                _rotationTimer -= Time.deltaTime;

            if(_isRotatingclockWise && RotationMismatch >0f  || !_isRotatingclockWise && RotationMismatch <0f){

                RotatePlayerToTarget();  
            }
                 
        }

        private void RotatePlayerToTarget(){
            Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x , 0f);
                transform.rotation= Quaternion.Lerp(transform.rotation , targetRotationX , modelRotationSpeed * Time.deltaTime);
        }

        private bool CanRun(){
            return _playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);
        }

        public void ResetLateralVelocity()
        {
            _chachedLateralVelocity = Vector3.zero;
        }
}
}