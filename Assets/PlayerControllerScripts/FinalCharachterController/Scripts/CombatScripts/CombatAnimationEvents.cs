using PrabuddhaSingh.FinalCharachterController;
using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class CombatAnimationEvents : MonoBehaviour
    {
        [SerializeField] private AttackHitbox leftFist;
        [SerializeField] private AttackHitbox rightFist;
        [SerializeField] private PlayerState playerState;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private EnergyBlast energyBlast;

        private void Awake()
        {
            if (playerState == null)
            {
                playerState = GetComponentInParent<PlayerState>();
            }

            if (playerCombat == null)
            {
                playerCombat = GetComponentInParent<PlayerCombat>();
            }
            if (playerHealth == null)
            {
                playerHealth = GetComponentInParent<PlayerHealth>();
            }
            if(energyBlast == null)
            {
                energyBlast = GetComponentInParent<EnergyBlast>();
            }
        }

        public void EnableRightPunch()
        {
            if (rightFist != null)
            {
                rightFist.EnableHitbox();
            }
        }
        public void DisableRightPunch()
        {
            if (rightFist != null)
            {
                rightFist.DisableHitbox();
            }
        }

        public void EnableLeftPunch()
        {
            if (leftFist != null)
            {
                leftFist.EnableHitbox();
            }
        }
        public void DisableLeftPunch()
        {
            if (leftFist != null)
            {
                leftFist.DisableHitbox();
            }
        }

        public void FinishAttack()
        {
            if (leftFist != null)
            {
                leftFist.DisableHitbox();
            }

            if (rightFist != null)
            {
                rightFist.DisableHitbox();
            }

            if (playerCombat != null)
            {
                playerCombat.ResetCombo();
                return;
            }

            playerState.SetPlayerActionState(PlayerActionStates.None);
        }

        public void OnHitAnimationFinished()
        {
            playerHealth.ClearHitState();
        }

        public void FinishChargedAttack()
        {
            playerState.ClearPlayerActionState();
            Debug.Log("Charged Attack Finished");
        }

        public void DoChargedAttack()
        {
            energyBlast.ExecuteBlast();
        }
    }
}

