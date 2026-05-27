using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
  public class EnemyHealth : MonoBehaviour , IDamageable{
   
   [SerializeField] private float maxHealth = 60f;
   [SerializeField] private float hitTransitionDuration = 0.03f;

   private float _currentHealth;
   private bool _isDead;
   private Animator _animator;
   private static readonly int HitStateHash = Animator.StringToHash("Hit");
   private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

   private void Awake()
    {
        _currentHealth = maxHealth; 
        _animator = GetComponentInChildren<Animator>();   
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
        _animator.SetBool(IsDeadHash, true); 

        Collider col = GetComponent<Collider>();
        col.enabled = false;
    }

    private void PlayHit()
    {
        _animator.CrossFadeInFixedTime(HitStateHash, hitTransitionDuration, 0, 0f);
    }
}  
}


