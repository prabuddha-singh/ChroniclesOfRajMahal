using UnityEngine;
using System;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class ManaController : MonoBehaviour
    {
        [SerializeField] private float maxMana = 100f;

        [SerializeField]private float currentMana;

        public float CurrentMana => currentMana;
        public float MaxMana => maxMana;

        public bool IsFull => currentMana >= maxMana;

        public event Action<int, int> OnManaUpdate;

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
            OnManaUpdate?.Invoke((int)currentMana, (int)maxMana);
            Debug.Log("Mana added: " + amount + ", Current Mana: " + currentMana);
        }

        public bool ConsumeMana()
        {
            if(!IsFull) return false;
            currentMana = 0f;
            OnManaUpdate?.Invoke((int)currentMana, (int)maxMana);
            return true;
        }
    }
}

