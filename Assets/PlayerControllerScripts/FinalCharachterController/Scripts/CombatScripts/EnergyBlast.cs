using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class EnergyBlast : MonoBehaviour
    {
        [SerializeField] private LayerMask blastLayers;
        [SerializeField] private float blastRadius = 5f;
        [SerializeField] private float blastDamage = 30f;
        [SerializeField] private float knockbackForce = 9f;
        [SerializeField] private float knockbackDuration = 0.5f;


        public void ExecuteBlast()
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, blastRadius,blastLayers,QueryTriggerInteraction.Ignore);
            
            foreach(Collider col in targets)
            {
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                IKnockbackable knockbackable = col.GetComponentInParent<IKnockbackable>();
                EnemyController enemyController = col.GetComponentInParent<EnemyController>();
                if(damageable != null)
                {
                    damageable.TakeDamage(blastDamage);
                    Debug.Log("Dealt damage to"+col.name);
                }
                if(knockbackable != null && enemyController !=null && enemyController._currentState != EnemyController.EnemyState.Dead)
                {
                    Vector3 direction = (col.transform.position - transform.position).normalized;
                    HitInfo hitInfo = new HitInfo{
                        Direction = direction,
                        Force = knockbackForce,
                        Duration = knockbackDuration
                    };
                    knockbackable.ApplyKnockback(hitInfo);
                }

                if(enemyController != null && enemyController._currentState == EnemyController.EnemyState.Dead)
                {
                    continue;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, blastRadius);
        }
    }
}

