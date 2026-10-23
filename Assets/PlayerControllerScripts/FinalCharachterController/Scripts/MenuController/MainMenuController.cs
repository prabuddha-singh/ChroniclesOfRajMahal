using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string GameplaySceneName = "Level_1";

        private GameObject mainMenuPanel;
        private GameObject howToPlayPanel;
        private GameObject aboutPanel;

        private void Awake()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            howToPlayPanel = FindDescendant(canvas.transform, "HowToPlayPanel")?.gameObject;
            aboutPanel = FindDescendant(canvas.transform, "AboutPanel")?.gameObject;
            mainMenuPanel = FindDescendant(canvas.transform, "MainMenuPanel")?.gameObject;

            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (aboutPanel != null) aboutPanel.SetActive(false);

            AddMenuAction("How to play", ShowHowToPlay);
            AddMenuAction("About", ShowAbout);

        }

        private static Transform FindDescendant(Transform root, string objectName)
        {
            if (root.name == objectName) return root;
            foreach (Transform child in root)
            {
                var found = FindDescendant(child, objectName);
                if (found != null) return found;
            }
            return null;
        }

        private void AddMenuAction(string objectName, UnityEngine.Events.UnityAction action)
        {
            var canvas = FindObjectOfType<Canvas>();
            var target = canvas == null ? null : FindDescendant(canvas.transform, objectName);
            var button = target == null ? null : target.GetComponent<Button>();
            if (button != null) button.onClick.AddListener(action);
        }

        public void ShowHowToPlay() => ShowInformationPanel(howToPlayPanel);
        public void ShowAbout() => ShowInformationPanel(aboutPanel);

        private void ShowInformationPanel(GameObject panel)
        {
            if (panel == null) return;
            SetMainMenuVisible(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(panel == howToPlayPanel);
            if (aboutPanel != null) aboutPanel.SetActive(panel == aboutPanel);
        }

        public void BackToMainMenu()
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (aboutPanel != null) aboutPanel.SetActive(false);
            SetMainMenuVisible(true);
        }

        private void SetMainMenuVisible(bool visible)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(visible);
        }

        public void StartGame()
        {
            SceneManager.LoadScene(GameplaySceneName);
        }

        public void QuitGame()
        {
            Application.Quit();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
