using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    private bool isPaused = false;
    
    // Use a more reliable input key (Escape is standard for pause menus)
    private KeyCode pauseKey = KeyCode.Escape;
    
    void Start()
    {
        Debug.Log("=== PAUSE MENU STARTED ===");
        Debug.Log("PauseMenu script is attached to: " + gameObject.name);
        
        if (pauseMenu != null)
        {
            Debug.Log("Pause menu reference is set to: " + pauseMenu.name);
            pauseMenu.SetActive(false);
            Debug.Log("Pause menu initially hidden");
        }
        else
        {
            Debug.LogError("PAUSE MENU REFERENCE IS NULL! Please assign in Inspector.");
        }
        
        // Ensure timescale is normal at start
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Check for pause key (Escape is standard)
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
        
        // Keep Space as backup but note it might conflict with other game actions
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - toggling pause");
            TogglePause();
        }
    }

    void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        Debug.Log("=== PAUSING GAME ===");
        
        if (pauseMenu == null)
        {
            Debug.LogError("CANNOT PAUSE: pauseMenu reference is null!");
            return;
        }

        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f; // This stops all time-based gameplay
        
        // Unlock cursor for menu interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Disable player input if needed
        DisablePlayerInput();
        
        Debug.Log("Game Paused - Time.timeScale: " + Time.timeScale);
    }
    
    public void Resume()
    {
        Debug.Log("=== RESUMING GAME ===");
        
        if (pauseMenu == null)
        {
            Debug.LogError("CANNOT RESUME: pauseMenu reference is null!");
            return;
        }

        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f; // Resume normal time
        
        // Lock cursor back for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Re-enable player input
        EnablePlayerInput();
        
        Debug.Log("Game Resumed - Time.timeScale: " + Time.timeScale);
    }
    
    private void DisablePlayerInput()
    {
        // Disable FirstPersonController
        FirstPersonController fpsController = FindObjectOfType<FirstPersonController>();
        if (fpsController != null)
        {
            fpsController.enabled = false;
        }
        
        // Disable interaction
        NT_PlayerInteract playerInteract = FindObjectOfType<NT_PlayerInteract>();
        if (playerInteract != null)
        {
            playerInteract.enabled = false;
        }
    }
    
    private void EnablePlayerInput()
    {
        // Re-enable FirstPersonController
        FirstPersonController fpsController = FindObjectOfType<FirstPersonController>();
        if (fpsController != null)
        {
            fpsController.enabled = true;
        }
        
        // Re-enable interaction
        NT_PlayerInteract playerInteract = FindObjectOfType<NT_PlayerInteract>();
        if (playerInteract != null)
        {
            playerInteract.enabled = true;
        }
    }
    
    public void Home()
    {
        Debug.Log("Loading Main Menu...");
        Time.timeScale = 1f; // Always reset timescale before loading scenes
        SceneManager.LoadScene("Main Menu");
    }
    
    public void Restart()
    {
        Debug.Log("Restarting level...");
        Time.timeScale = 1f; // Always reset timescale before loading scenes
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void Quit()
    {
        Debug.Log("Quitting the game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Ensure timescale is reset when this object is destroyed
    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
    
    // Ensure timescale is reset when application loses focus (optional)
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isPaused)
        {
            // Auto-pause when game loses focus
            Pause();
        }
    }
}