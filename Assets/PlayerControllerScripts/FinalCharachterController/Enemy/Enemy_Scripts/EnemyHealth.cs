using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
  public class EnemyHealth : MonoBehaviour , IDamageable{
   
   [SerializeField] private float maxHealth = 60f;
   [SerializeField] private float hitLockDuration = 0.6f;
   [SerializeField] private EnemyController _enemyController;

   private float _currentHealth;
   private bool _isDead;
   private Animator _animator;
   private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
   private static readonly int HitStateHash = Animator.StringToHash("Hit");
   

   private void Awake()
    {
        _currentHealth = maxHealth; 
        _animator = GetComponentInChildren<Animator>(); 
        _enemyController = GetComponent<EnemyController>();  
    }

    public void TakeDamage(float damage)
    {
        if(_isDead) return;

        _currentHealth -= damage;
        if(_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayHit();    
        }   
    }

    private void Die()
    {
        _isDead = true;
        _animator.ResetTrigger(HitTriggerHash);
        _enemyController.ChangeState(EnemyController.EnemyState.Dead);

        Collider col = GetComponent<Collider>();
        if(col != null)
        {
            col.enabled = false;
        }
    }

    private void PlayHit()
    {
        if(IsHitReactionPlaying())
        {
            _animator.ResetTrigger(HitTriggerHash);
            return;
        }

        if(!_enemyController.TryPlayHitReaction(hitLockDuration))
        {
            _animator.ResetTrigger(HitTriggerHash);
            return;
        }

        _animator.ResetTrigger(HitTriggerHash);
        _animator.SetTrigger(HitTriggerHash);
    }

    private bool IsHitReactionPlaying()
    {
        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
        if(currentState.shortNameHash == HitStateHash) return true;

        if(!_animator.IsInTransition(0)) return false;

        AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
        return nextState.shortNameHash == HitStateHash;
    }
}  
}


