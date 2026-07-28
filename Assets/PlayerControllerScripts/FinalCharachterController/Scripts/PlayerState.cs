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

   public enum PlayerActionStates
    {
        None = 0,
        Attacking = 1,
        Gathering  = 2,
        Hit = 3,
    }
    public class PlayerState : MonoBehaviour
{
    [field:SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
    [field:SerializeField] public PlayerActionStates CurrentPlayerActionState { get; private set; } = PlayerActionStates.None;
    public void SetPlayerMovementState(PlayerMovementState playerMovementState){
         CurrentPlayerMovementState = playerMovementState;
    }

    public void SetPlayerActionState(PlayerActionStates playerActionState){
        CurrentPlayerActionState = playerActionState;
    }

    public void ClearPlayerActionState(){
        CurrentPlayerActionState = PlayerActionStates.None;
    }

    public bool IsPlayingAction(){
        return CurrentPlayerActionState != PlayerActionStates.None;
    }

    public bool InGroundedState(){
        return IsStateGroundedState(CurrentPlayerMovementState);
    }

    public bool BlocksMovement()
        {
            return CurrentPlayerActionState == PlayerActionStates.Gathering || CurrentPlayerActionState == PlayerActionStates.Hit;
        }

    public bool IsStateGroundedState(PlayerMovementState movementState){
            return movementState == PlayerMovementState.running || 
                 movementState == PlayerMovementState.sprinting ||
                 movementState == PlayerMovementState.walking ||
                 movementState == PlayerMovementState.Idling ;
    }
  
}
}
