using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("ClassSelection", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
