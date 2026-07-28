using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
public class EnemyCombatEvents : MonoBehaviour
{
   [SerializeField] private EnemyAttackHitbox RightFist;

   public void EnableRightPunch()
    {
       if(RightFist !=  null) RightFist.EnableEnemyHitBox();     
    }

    public void DisableRightPunch()
    {
       if(RightFist !=null) RightFist.DisableEnemyHitBox();
    }
}

}