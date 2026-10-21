using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string GameplaySceneName = "Level_1";

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

