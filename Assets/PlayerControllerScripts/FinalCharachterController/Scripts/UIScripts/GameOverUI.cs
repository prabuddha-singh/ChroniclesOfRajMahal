using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class GameOverUI : MonoBehaviour
    {
        public static GameOverUI Instance { get; private set; }
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField, Min(0.01f)] private float fadeDuration = 1f;

        private bool isGameOver = false;

        private PlayerLocomotionInput playerLocomotionInput;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            Instance = this;
            if (gameOverPanel == null)
            {
                Debug.Log("GameOverPanel is not assigned in the inspector.");
            }
            playerLocomotionInput = FindFirstObjectByType<PlayerLocomotionInput>();
            gameOverPanel.SetActive(false);
            canvasGroup.alpha = 0f;
        }

        public void ShowGameOverUI()
        {
            if(isGameOver) return;

            isGameOver = true;
            playerLocomotionInput.isLookDisabled = true;
            gameOverPanel.SetActive(true);
            StartCoroutine(GameOverSequence());
        }

        private IEnumerator GameOverSequence()
        {
            gameOverPanel.SetActive(true);
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while(elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            Time.timeScale = 0f;
        }

        public void RestartGame()
        {
            Time.timeScale=1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            if(Instance == this)
            {
                Instance = null;
            }
        }
    }
}

