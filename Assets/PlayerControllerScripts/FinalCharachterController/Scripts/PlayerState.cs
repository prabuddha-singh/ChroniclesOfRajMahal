using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController{

     public enum PlayerMovementState {
    Idling =0,
    walking =1,
    running =2,
    sprinting=3,
    jumping =4,
    falling=5,
    strafing =6,
   }
    public class PlayerState : MonoBehaviour
{
    [field:SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState){
         CurrentPlayerMovementState = playerMovementState;
    }

    public bool InGroundedState(){
        return IsStateGroundedState(CurrentPlayerMovementState);
    }

    public bool IsStateGroundedState(PlayerMovementState movementState){
            return movementState == PlayerMovementState.running || 
                 movementState == PlayerMovementState.sprinting ||
                 movementState == PlayerMovementState.walking ||
                 movementState == PlayerMovementState.Idling ;
    }
  
}
}
