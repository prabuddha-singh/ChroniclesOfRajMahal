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
    public class PlayerController : MonoBehaviour
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
    public float gravity =25f;
    public float jumpSpeed=0.01f;
    public float antiBump = 0f;
    public float terminalVelocity =50f;
    
   
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
    private bool _jumpedLastFrame= false;
    private float _stepOffSet ;
    private PlayerMovementState _lastMovementState = PlayerMovementState.falling;

    private PlayerState _playerState;

        void Awake()
        {
            _playerLocomotionInput  = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
            antiBump = sprintSpeed;
            _stepOffSet = _characterController.stepOffset;
        }

        private void Update(){
            UpdateMovementState();            // prioritize updating movement state 
            HandleVerticalMovement();      // first handle vertical movememt 
            HandleLateralMovement();        // then handle lateral movement 
        }

        private void UpdateMovementState(){
            _lastMovementState= _playerState.CurrentPlayerMovementState;

            bool canRun = CanRun();
            bool isMoveInput = _playerLocomotionInput.MovementInput != Vector2.zero;                  //order in which
            bool isMoveLaterally = IsMoveLaterally();                                                 // this code is written
            bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMoveLaterally;             // matters here 
            bool isWalking = isMoveLaterally && (!canRun || _playerLocomotionInput.WalkToggledOn);    // cause each state is checked 
            bool isGrounded = IsGrounded();                                                           // for the player accordingly 

            PlayerMovementState lateralstate = isWalking ? PlayerMovementState.walking :
                                              isSprinting ? PlayerMovementState.sprinting :
                                              isMoveLaterally || isMoveInput ? PlayerMovementState.running : PlayerMovementState.Idling;
                                               _playerState.SetPlayerMovementState(lateralstate); 

            if((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y >0f){
                _playerState.SetPlayerMovementState(PlayerMovementState.jumping);
                _jumpedLastFrame=false;
                _characterController.stepOffset= 0f;
            }
            else if((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <0f){
                _playerState.SetPlayerMovementState(PlayerMovementState.falling);
                _jumpedLastFrame=false;
                _characterController.stepOffset= 0f;   
            }
            else{
                _characterController.stepOffset = _stepOffSet;
            }
        }

        private void HandleVerticalMovement(){
             bool isGrounded = _playerState.InGroundedState();

              _verticalVelocity -= gravity * Time.deltaTime;

            if(isGrounded && _verticalVelocity <0){
                _verticalVelocity = -antiBump;
            }

            if(isGrounded && _playerLocomotionInput.JumpPressed){
                _verticalVelocity +=  Mathf.Sqrt(jumpSpeed * 3 * gravity);
                _jumpedLastFrame = true;
            }

            if(_playerState.IsStateGroundedState(_lastMovementState) && !isGrounded){
                    _verticalVelocity += antiBump;
            }

            if(Mathf.Abs(_verticalVelocity)> Mathf.Abs(terminalVelocity)){
                _verticalVelocity = -1f *Mathf.Abs(terminalVelocity);
            }

        }
        private void HandleLateralMovement(){
              bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.sprinting;
              bool isGrounded = _playerState.InGroundedState();
              bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.walking;

              float lateralAcceleration =!isGrounded ? inAirAcceleration :
               isWalking ? walkAcceleration : 
               isSprinting ? sprintAcceleration : acceleration;


              float clampLateralMagnitude =!isGrounded ? sprintSpeed :
                                            isWalking ? walkSpeed : 
                                            isSprinting? sprintSpeed : speed;

            Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x,0f,_playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXz = new Vector3(_playerCamera.transform.right.x,0f,_playerCamera.transform.right.z).normalized;
            Vector3 movementDirection =cameraRightXz*_playerLocomotionInput.MovementInput.x + cameraForwardXZ* _playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
            Vector3 newVelocity = _characterController.velocity + movementDelta;

            Vector3 currentdrag= newVelocity.normalized * drag * Time.deltaTime;
            newVelocity=(newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentdrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x , 0f , newVelocity.z), clampLateralMagnitude);
            newVelocity.y += _verticalVelocity;
            newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;

            _characterController.Move(newVelocity * Time.deltaTime);
        }

        private Vector3 HandleSteepWalls(Vector3 velocity){
             Vector3 normal = PlayerControlUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
             float angle = Vector3.Angle(normal, Vector3.up);
             bool validAngle = angle <= _characterController.slopeLimit;
             if(!validAngle && _verticalVelocity <0){
                velocity = Vector3.ProjectOnPlane(normal , velocity);
             }

             return velocity;
        }

        private bool IsMoveLaterally(){
           Vector3 lateralVelocity = new Vector3(_characterController.velocity.x ,0f , _characterController.velocity.z);

           return lateralVelocity.magnitude >movingThreshold ;  
        }

        private bool IsGrounded(){
            bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
            return grounded;
        }

        private bool IsGroundedWhileGrounded(){
            Vector3 spherePosition = new Vector3(transform.position.x , transform.position.y - _characterController.radius , transform.position.z);
            bool grounded = Physics.CheckSphere(spherePosition , _characterController.radius , _groundLayers , QueryTriggerInteraction.Ignore);
            return grounded;
        }

        private bool IsGroundedWhileAirborne(){
            Vector3 normal = PlayerControlUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
             float angle = Vector3.Angle(normal, Vector3.up);
             bool validAngle = angle <= _characterController.slopeLimit;
             print(angle);
            return _characterController.isGrounded && validAngle;
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
}
}