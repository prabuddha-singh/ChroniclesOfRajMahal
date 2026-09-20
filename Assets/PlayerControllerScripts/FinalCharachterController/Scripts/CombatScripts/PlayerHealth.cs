using System;
using System.Collections;
using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public event Action<int, int> OnHealthChanged;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float hitRecoveryDuration = 1.1f;
        [SerializeField] private PlayerState _playerState;
        [SerializeField] private PlayerCombat _playerCombat;
        private Coroutine hitRecoveryRoutine;

        private void Awake()
        {
            currentHealth = maxHealth;
            _playerState = GetComponent<PlayerState>();
            _playerCombat = GetComponent<PlayerCombat>();
        }

        private void Start()
        {
           OnHealthChanged?.Invoke((int)currentHealth,(int)maxHealth);
        }

        public void TakeDamage(float damage)
        {
            _playerCombat.ResetCombo();
            PlayHitAnimation();
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            OnHealthChanged?.Invoke((int)currentHealth, (int)maxHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void PlayHitAnimation()
        {
            _playerState.SetPlayerActionState(PlayerActionStates.Hit);

            if(hitRecoveryRoutine != null)
            {
                StopCoroutine(hitRecoveryRoutine);
            }

            hitRecoveryRoutine = StartCoroutine(ClearHitStateAfterDelay());
        }

        private IEnumerator ClearHitStateAfterDelay()
        {
            yield return new WaitForSeconds(hitRecoveryDuration);
            ClearHitState();
            hitRecoveryRoutine = null;
        }

        public void ClearHitState()
        {
            if(_playerState.CurrentPlayerActionState == PlayerActionStates.Hit)
            {
                _playerState.ClearPlayerActionState();
            }
        }

        private void Die()
        {
        }
    }
}
