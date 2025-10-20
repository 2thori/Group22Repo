using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool PausedGame = false;
    public GameObject pauseMenu;

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape Pressed");
            
            if (PausedGame)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    { 
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        PausedGame = true;  
    }

    void Pause()
    { 
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        PausedGame = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuInteractive");
    }

    public void QuitGame()
    { 
        Application.Quit();
    }
}
