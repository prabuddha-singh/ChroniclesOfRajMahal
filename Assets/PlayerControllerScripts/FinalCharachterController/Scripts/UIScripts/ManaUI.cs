using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class ManaUI : MonoBehaviour
    {
        [SerializeField] private ManaController _manaController;
        [SerializeField] private TMPro.TextMeshProUGUI _manaText;

        private void OnEnable()
        {
            _manaController.OnManaUpdate += UpdateManaUI;
        }

        private void OnDisable()
        {
            _manaController.OnManaUpdate -= UpdateManaUI;
        }

        private void UpdateManaUI(int currentMana, int maxMana)
        {
            _manaText.text = $"MANA : {currentMana} / {maxMana}";
        }
    }
}

