using UnityEngine;
using TMPro;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class PlayerHealthUI : MonoBehaviour
    {

        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private TextMeshProUGUI healthText;

        private void OnEnable()
        {
            playerHealth.OnHealthChanged+= UpdateHealthUI;
        }

        private void OnDisable()
        {
            playerHealth.OnHealthChanged -= UpdateHealthUI;
        }

        private void UpdateHealthUI(int currentHealth , int maxHealth)
        {
            if(currentHealth <= maxHealth * 0.25f)
            {
                healthText.color = Color.red;
            }
            if(currentHealth <= maxHealth * 0.5f && currentHealth > maxHealth * 0.25f)
            {
                healthText.color = Color.yellow;
            }
            healthText.text = $"HP : {currentHealth} / {maxHealth}";
        }

    }
}


