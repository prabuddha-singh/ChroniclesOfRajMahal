using System;
using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class PlayerHealth : MonoBehaviour , IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private PlayerState _playerState;
    [SerializeField] private PlayerCombat _playerCombat;

    private void Awake()
    {
        currentHealth = maxHealth;
        _playerState = GetComponent<PlayerState>();
        _playerCombat = GetComponent<PlayerCombat>();
    }

    public void TakeDamage(float damage)
    {
        _playerCombat.ResetCombo();
        PlayHitAnimation();
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void PlayHitAnimation(){
        _playerState.SetPlayerActionState(PlayerActionStates.Hit); 
        Debug.Log(_playerState.CurrentPlayerActionState);  
    }

    private void Die()
    {
        Debug.Log("Player has died.");
    }
}
}
