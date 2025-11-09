using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] public static bool PausedGame = false;

    [SerializeField] public GameObject pauseMenu;
    [SerializeField] public GameObject gameHUD;
    [SerializeField] public GameObject fpsHUD;

    [SerializeField] public KeyCode pauseKey = KeyCode.Escape;

    public void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
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
        Time.timeScale = 1f;
        gameHUD.SetActive(true);
        fpsHUD.SetActive(true);
        pauseMenu.SetActive(false);
        PausedGame = false;
        //Cursor.lockState = CursorLockMode.Locked;

    }

    public void Pause()
    {
        Time.timeScale = 0f;
        gameHUD.SetActive(false);
        fpsHUD.SetActive(false);
        pauseMenu.SetActive(true);
        PausedGame = true;
        //Cursor.lockState = CursorLockMode.None;
        
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    { 
        Application.Quit();
    }
}
