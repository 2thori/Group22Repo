using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameHUD;
    [SerializeField] private GameObject fpsHUD;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button optionsButton;

    [Header("Audio Objects")]
    //[SerializeField]

    [Header("Is the Game Paused?")]
    [SerializeField] public bool PausedGame = false;
    public static PauseMenu Instance { get; private set; }

    [SerializeField] public KeyCode pauseKey = KeyCode.Escape;

    private void Awake()
    {
        Instance = this;
        SetupButtonListeners();
    }

    public void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    void SetupButtonListeners()
    { 
        resumeButton.onClick.AddListener(Resume);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void TogglePause()
    { 
        PausedGame = !PausedGame;

        pauseMenu.SetActive(PausedGame);
        gameHUD.SetActive(!PausedGame);

        Time.timeScale = PausedGame ? 0 : 1;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.enabled = !PausedGame;

        Cursor.lockState = PausedGame ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = PausedGame;

    }

    public void Resume()
    { 
        /* Basic Resume functionality. Not functional ATM.
        Time.timeScale = 1f;
        gameHUD.SetActive(true);
        fpsHUD.SetActive(true);
        pauseMenu.SetActive(false);
        PausedGame = false;
        Cursor.lockState = CursorLockMode.Locked;
        */
        if (PausedGame) TogglePause();
    }

    /* Another failed attempt
    public void Pause()
    {
        Time.timeScale = 0f;
        gameHUD.SetActive(false);
        fpsHUD.SetActive(false);
        pauseMenu.SetActive(true);
        PausedGame = true;
        Cursor.lockState = CursorLockMode.None;
    }*/

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
