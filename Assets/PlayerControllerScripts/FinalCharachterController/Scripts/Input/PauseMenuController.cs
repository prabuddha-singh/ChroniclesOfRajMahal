using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class PauseMenuController : MonoBehaviour, PlayerControls.IPauseActionMapActions
    {
        [SerializeField] private GameObject PauseMenuUI;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool isPaused = false;
        private void OnEnable()
        {
            if(PlayerInputManager.Instance?.PlayerControls == null) return;

            PlayerInputManager.Instance.PlayerControls.PauseActionMap.SetCallbacks(this);
            PlayerInputManager.Instance.PlayerControls.PauseActionMap.Enable();
            
        }

        private void Start()
        {
            PauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            if(PlayerInputManager.Instance?.PlayerControls == null) return;

            PlayerInputManager.Instance.PlayerControls.PauseActionMap.RemoveCallbacks(this);
        }

        public void OnPause(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void ResumeGame()
        {
            PauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }

        private void PauseGame()
        { 
            PauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }

        public void QuitToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}

