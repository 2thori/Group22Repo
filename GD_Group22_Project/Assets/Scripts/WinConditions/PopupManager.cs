using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PopupManager : MonoBehaviour
{
    [System.Serializable]
    public class AreaPopup
    {
        public string areaName;
        public string popupTitle;
        [TextArea(3, 5)]
        public string popupDescription;
    }

    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button closeButton;

    [Header("UI Elements to Hide During Popup")]
    [SerializeField] private List<GameObject> uiElementsToHide = new List<GameObject>();

    [Header("Area Popups")]
    [SerializeField] private List<AreaPopup> areaPopups = new List<AreaPopup>();

    private bool isPopupActive = false;
    private HashSet<string> shownPopups = new HashSet<string>();
    private List<bool> uiElementOriginalStates = new List<bool>();

    public static PopupManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Hide popup initially
        if (popupPanel != null)
            popupPanel.SetActive(false);

        // Setup close button
        if (closeButton != null)
            closeButton.onClick.AddListener(HidePopup);

        // Load which popups have been shown
        LoadPopupStates();
    }

    private void Update()
    {
        // Allow closing popup with Escape key
        if (isPopupActive && Input.GetKeyDown(KeyCode.Escape))
        {
            HidePopup();
        }
    }

    public void ShowAreaPopup(string areaName)
    {
        // Check if this popup has already been shown
        if (shownPopups.Contains(areaName))
        {
            Debug.Log($"Popup for {areaName} has already been shown. Skipping.");
            return;
        }

        // Find the area popup
        AreaPopup areaPopup = areaPopups.Find(popup => popup.areaName == areaName);

        if (areaPopup != null)
        {
            ShowPopup(areaPopup.popupTitle, areaPopup.popupDescription);
            shownPopups.Add(areaName);
            SavePopupStates();
        }
        else
        {
            Debug.LogWarning($"No popup found for area: {areaName}");
        }
    }

    private void ShowPopup(string title, string description)
    {
        // Store current UI states and hide other UI elements
        uiElementOriginalStates.Clear();
        foreach (GameObject uiElement in uiElementsToHide)
        {
            if (uiElement != null)
            {
                uiElementOriginalStates.Add(uiElement.activeSelf);
                uiElement.SetActive(false);
            }
        }

        // Show popup content
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            isPopupActive = true;
            
            // Pause the game
            Time.timeScale = 0f;
            
            // Unlock and show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log($"Showing popup: {title}");
        }
    }

    public void HidePopup()
    {
        // Restore other UI elements to their original state
        for (int i = 0; i < uiElementsToHide.Count; i++)
        {
            if (uiElementsToHide[i] != null && i < uiElementOriginalStates.Count)
            {
                uiElementsToHide[i].SetActive(uiElementOriginalStates[i]);
            }
        }

        // Hide popup
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            isPopupActive = false;
            
            // Resume the game
            Time.timeScale = 1f;
            
            // Lock and hide cursor (if you're using FPS controller)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Popup closed");
        }
    }

    private void SavePopupStates()
    {
        // Save all shown popups to PlayerPrefs
        int index = 0;
        foreach (string areaName in shownPopups)
        {
            PlayerPrefs.SetString($"Popup_Shown_{index}", areaName);
            index++;
        }
        PlayerPrefs.SetInt("Popup_Count", shownPopups.Count);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved {shownPopups.Count} popup states");
    }

    private void LoadPopupStates()
    {
        shownPopups.Clear();
        
        int popupCount = PlayerPrefs.GetInt("Popup_Count", 0);
        for (int i = 0; i < popupCount; i++)
        {
            string areaName = PlayerPrefs.GetString($"Popup_Shown_{i}", "");
            if (!string.IsNullOrEmpty(areaName))
            {
                shownPopups.Add(areaName);
            }
        }
        
        Debug.Log($"Loaded {shownPopups.Count} previously shown popups");
    }

    // Method to reset all popups (for testing or new game)
    public void ResetAllPopups()
    {
        shownPopups.Clear();
        PlayerPrefs.DeleteKey("Popup_Count");
        
        // Clear all popup keys
        for (int i = 0; i < 100; i++) // Reasonable upper limit
        {
            if (PlayerPrefs.HasKey($"Popup_Shown_{i}"))
            {
                PlayerPrefs.DeleteKey($"Popup_Shown_{i}");
            }
            else
            {
                break;
            }
        }
        PlayerPrefs.Save();
        
        Debug.Log("All popups reset - will show again on next trigger");
    }

    public bool IsPopupActive()
    {
        return isPopupActive;
    }

    public bool HasPopupBeenShown(string areaName)
    {
        return shownPopups.Contains(areaName);
    }

    // Method to manually add UI elements to hide (useful for dynamic UI)
    public void AddUIElementToHide(GameObject uiElement)
    {
        if (!uiElementsToHide.Contains(uiElement))
        {
            uiElementsToHide.Add(uiElement);
        }
    }

    // Method to remove UI element from hide list
    public void RemoveUIElementFromHide(GameObject uiElement)
    {
        if (uiElementsToHide.Contains(uiElement))
        {
            uiElementsToHide.Remove(uiElement);
        }
    }
}