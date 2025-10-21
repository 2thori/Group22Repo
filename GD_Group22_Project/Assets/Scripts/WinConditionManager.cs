using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class WinConditionManager : MonoBehaviour
{
    [Header("Win Objects")]
    [SerializeField] private string winObject1Name = "Final Artifact";
    [SerializeField] private string winObject2Name = "Ancient Relic";

    [Header("Win Panel UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winMessageText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string winMessage = "Congratulations! You've completed your journey!";

    [Header("UI Elements to Disable")]
    [SerializeField] private GameObject uiElement1; // First UI element to disable
    [SerializeField] private GameObject uiElement2; // Second UI element to disable

    [Header("Audio")]
    [SerializeField] private AudioClip winSound;

    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject;

    private bool object1Interacted = false;
    private bool object2Interacted = false;
    private bool gameWon = false;

    // Store original UI states
    private bool uiElement1OriginalState;
    private bool uiElement2OriginalState;

    public static WinConditionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeWinPanel();
        CacheUIStates();
        
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        
        Debug.Log("WinConditionManager started. Ready to disable 2 UI elements.");
    }

    private void CacheUIStates()
    {
        // Cache the original active states of UI elements
        if (uiElement1 != null)
        {
            uiElement1OriginalState = uiElement1.activeSelf;
            Debug.Log($"UI Element 1 original state: {uiElement1OriginalState}");
        }
        else
        {
            Debug.LogError("UI Element 1 is not assigned!");
        }

        if (uiElement2 != null)
        {
            uiElement2OriginalState = uiElement2.activeSelf;
            Debug.Log($"UI Element 2 original state: {uiElement2OriginalState}");
        }
        else
        {
            Debug.LogError("UI Element 2 is not assigned!");
        }
    }

    private void InitializeWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        if (winMessageText != null)
        {
            winMessageText.text = winMessage;
        }
    }

    public void RegisterWinObjectInteraction(WinObject.WinObjectType objectType)
    {
        if (gameWon) return;

        switch (objectType)
        {
            case WinObject.WinObjectType.Object1:
                object1Interacted = true;
                Debug.Log($"Win Object 1 collected! Progress: {GetInteractedCount()}/2");
                break;
            case WinObject.WinObjectType.Object2:
                object2Interacted = true;
                Debug.Log($"Win Object 2 collected! Progress: {GetInteractedCount()}/2");
                break;
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (object1Interacted && object2Interacted && !gameWon)
        {
            gameWon = true;
            WinGame();
        }
    }

    private int GetInteractedCount()
    {
        int count = 0;
        if (object1Interacted) count++;
        if (object2Interacted) count++;
        return count;
    }

    private void WinGame()
    {
        Debug.Log("🎉 YOU WIN! All required objects have been collected!");

        if (winPanel != null)
        {
            // Disable the 2 UI elements
            DisableUIElements();
            
            // Show win panel
            winPanel.SetActive(true);
            
            if (winSound != null)
            {
                AudioSource.PlayClipAtPoint(winSound, Camera.main.transform.position);
            }

            DisablePlayerInput();
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Debug.Log("Win panel active. 2 UI elements should be disabled.");
        }
    }

    private void DisableUIElements()
    {
        Debug.Log("Disabling UI elements...");
        
        if (uiElement1 != null)
        {
            uiElement1.SetActive(false);
            Debug.Log($"Disabled UI Element 1: {uiElement1.name}");
        }

        if (uiElement2 != null)
        {
            uiElement2.SetActive(false);
            Debug.Log($"Disabled UI Element 2: {uiElement2.name}");
        }
    }

    private void EnableUIElements()
    {
        Debug.Log("Re-enabling UI elements...");
        
        if (uiElement1 != null)
        {
            uiElement1.SetActive(uiElement1OriginalState);
            Debug.Log($"Re-enabled UI Element 1: {uiElement1.name} to {uiElement1OriginalState}");
        }

        if (uiElement2 != null)
        {
            uiElement2.SetActive(uiElement2OriginalState);
            Debug.Log($"Re-enabled UI Element 2: {uiElement2.name} to {uiElement2OriginalState}");
        }
    }

    private void DisablePlayerInput()
    {
        if (playerObject != null)
        {
            FirstPersonController fpsController = playerObject.GetComponent<FirstPersonController>();
            if (fpsController != null)
            {
                fpsController.enabled = false;
            }

            NT_PlayerInteract playerInteract = playerObject.GetComponent<NT_PlayerInteract>();
            if (playerInteract != null)
            {
                playerInteract.enabled = false;
            }
        }
    }

    private void EnablePlayerInput()
    {
        if (playerObject != null)
        {
            FirstPersonController fpsController = playerObject.GetComponent<FirstPersonController>();
            if (fpsController != null)
            {
                fpsController.enabled = true;
            }

            NT_PlayerInteract playerInteract = playerObject.GetComponent<NT_PlayerInteract>();
            if (playerInteract != null)
            {
                playerInteract.enabled = true;
            }
        }
    }

    public void GoToMainMenu()
    {
        Debug.Log("Loading Main Menu...");
        
        EnablePlayerInput();
        EnableUIElements();
        
        SceneManager.LoadScene("MainMenu");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    [ContextMenu("Reset Win Condition")]
    public void ResetWinCondition()
    {
        object1Interacted = false;
        object2Interacted = false;
        gameWon = false;
        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        EnablePlayerInput();
        EnableUIElements();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Win condition reset!");
    }

    public bool IsObject1Interacted() { return object1Interacted; }
    public bool IsObject2Interacted() { return object2Interacted; }
    public int GetTotalInteracted() { return GetInteractedCount(); }
}