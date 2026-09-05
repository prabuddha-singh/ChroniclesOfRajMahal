using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private TMPro.TextMeshProUGUI waveText;

        private void OnEnable()
        {
            waveManager.OnWaveStarted += UpdateWaveUI;
        }

        private void OnDisable()
        {
            waveManager.OnWaveStarted -= UpdateWaveUI;
        }

        private void UpdateWaveUI(int currentWave, int maxWaves)
        {

            waveText.text = $"Wave: {currentWave} / {maxWaves}";

        }
    }
}


