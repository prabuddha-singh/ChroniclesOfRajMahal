using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class ManaController : MonoBehaviour
    {
        [SerializeField] private float maxMana = 100f;

        [SerializeField]private float currentMana;

        public float CurrentMana => currentMana;
        public float MaxMana => maxMana;

        public bool IsFull => currentMana >= maxMana;

        private void Awake()
        {
            currentMana = 0f;
        }

        public void AddMana(float amount)
        {
            if (amount < 0f)
            {
                return;
            }

            currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
            Debug.Log("Mana added: " + amount + ", Current Mana: " + currentMana);
        }

        public bool ConsumeMana()
        {
            if(!IsFull) return false;
            currentMana = 0f;
            return true;
        }
    }
}

