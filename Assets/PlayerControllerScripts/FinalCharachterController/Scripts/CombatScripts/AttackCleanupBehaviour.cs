using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
   public class AttackCleanupBehaviour : StateMachineBehaviour{

    [SerializeField]private int attackIndex;

    public override void OnStateExit(Animator animator , AnimatorStateInfo stateInfo , int layerIndex)
    {
        PlayerCombat combat = animator.GetComponentInParent<PlayerCombat>();
        if(combat == null)
        {
           return;
        }

        if(combat.CurrentComboIndex <= attackIndex)
        {
           combat.ResetCombatState();     
        }
    }
    
} 
}

