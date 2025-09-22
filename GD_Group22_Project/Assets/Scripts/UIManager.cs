using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Key UI Elements")] 
    [SerializeField] private Transform keyPanel;
    [SerializeField] private GameObject keyImagePrefab;
    
    [Header("Gravity Gun Parts UI Elements")]
    [SerializeField] private Transform gravityGunPartsPanel;
    [SerializeField] private GameObject gravityGunPartImagePrefab;
    [SerializeField] private TextMeshProUGUI partsProgressText;
    
    [Header("Settings")]
    [SerializeField] private bool debugMode = true;

    private Dictionary<Key, GameObject> keyImages = new Dictionary<Key, GameObject>();
    private Dictionary<GravityGunPart, GameObject> gravityGunPartImages = new Dictionary<GravityGunPart, GameObject>();
    
    // Singleton pattern
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("UIManager");
                    instance = obj.AddComponent<UIManager>();
                    Debug.LogWarning("UIManager instance created dynamically.");
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Initialize components
        InitializeUIComponents();
    }

    private void InitializeUIComponents()
    {
        // Check if key panel is assigned
        if (keyPanel == null)
        {
            Debug.LogError("KeyPanel is not assigned in the inspector!");
            keyPanel = GameObject.Find("KeyPanel")?.transform;
        }

        // Check if key image prefab is assigned
        if (keyImagePrefab == null)
        {
            Debug.LogError("KeyImagePrefab is not assigned in the inspector!");
            keyImagePrefab = Resources.Load<GameObject>("KeyImagePrefab");
        }
        
        // Check if gravity gun parts panel is assigned
        if (gravityGunPartsPanel == null)
        {
            Debug.LogError("GravityGunPartsPanel is not assigned in the inspector!");
            gravityGunPartsPanel = GameObject.Find("GravityGunPartsPanel")?.transform;
        }

        // Check if gravity gun part image prefab is assigned
        if (gravityGunPartImagePrefab == null)
        {
            Debug.LogError("GravityGunPartImagePrefab is not assigned in the inspector!");
            gravityGunPartImagePrefab = Resources.Load<GameObject>("GravityGunPartImagePrefab");
        }
        
        // Check if parts progress text is assigned
        if (partsProgressText == null)
        {
            Debug.LogError("PartsProgressText is not assigned in the inspector!");
            partsProgressText = GameObject.Find("PartsProgressText")?.GetComponent<TextMeshProUGUI>();
        }
        
        // Hide the parts panel initially if it's empty
        if (gravityGunPartsPanel != null && gravityGunPartsPanel.gameObject.activeSelf)
        {
            gravityGunPartsPanel.gameObject.SetActive(false);
        }
    }

    // Key UI Methods
    public void AddKeyToUI(Key key)
    {
        if (key == null)
        {
            Debug.LogError("Key is null! Cannot add to UI.");
            return;
        }

        if (keyImages.ContainsKey(key))
        {
            if (debugMode) Debug.LogWarning("Key already exists in UI: " + key.keyName);
            return;
        }

        if (keyImagePrefab == null || keyPanel == null)
        {
            Debug.LogError("Key UI elements not properly initialized!");
            return;
        }

        if (key.keySprite == null)
        {
            Debug.LogWarning("Key sprite is null for: " + key.keyName);
        }

        GameObject keyImage = Instantiate(keyImagePrefab, keyPanel);
        Image imageComponent = keyImage.GetComponent<Image>();
        
        if (imageComponent != null)
        {
            imageComponent.sprite = key.keySprite;
        }
        else
        {
            Debug.LogError("KeyImagePrefab doesn't have an Image component!");
            Destroy(keyImage);
            return;
        }
        
        keyImages[key] = keyImage;

        if (debugMode)
        {
            Debug.Log("Added key to UI: " + key.keyName);
        }
    }
    
    public void RemoveKeyFromUI(Key key)
    {
        if (keyImages.ContainsKey(key))
        {
            Destroy(keyImages[key]);
            keyImages.Remove(key);
            
            if (debugMode)
            {
                Debug.Log("Removed key from UI: " + key.keyName);
            }
        }
    }
    
    public void ClearAllKeys()
    {
        foreach (GameObject keyImage in keyImages.Values)
        {
            Destroy(keyImage);
        }
        keyImages.Clear();
        
        if (debugMode)
        {
            Debug.Log("Cleared all keys from UI");
        }
    }

    // Gravity Gun Parts UI Methods
    public void AddGravityGunPartToUI(GravityGunPart part, int collectedCount, int totalCount)
    {
        if (part == null)
        {
            Debug.LogError("Part is null! Cannot add to UI.");
            return;
        }

        if (gravityGunPartImages.ContainsKey(part))
        {
            if (debugMode) Debug.LogWarning("Part already exists in UI: " + part.partName);
            return;
        }

        if (gravityGunPartImagePrefab == null || gravityGunPartsPanel == null)
        {
            Debug.LogError("Gravity gun parts UI elements not properly initialized!");
            return;
        }

        // Show the panel when adding the first part
        if (gravityGunPartImages.Count == 0 && gravityGunPartsPanel != null)
        {
            gravityGunPartsPanel.gameObject.SetActive(true);
        }

        if (part.partSprite == null)
        {
            Debug.LogWarning("Part sprite is null for: " + part.partName);
        }

        GameObject partImage = Instantiate(gravityGunPartImagePrefab, gravityGunPartsPanel);
        Image imageComponent = partImage.GetComponent<Image>();
        
        if (imageComponent != null)
        {
            imageComponent.sprite = part.partSprite;
        }
        else
        {
            Debug.LogError("GravityGunPartImagePrefab doesn't have an Image component!");
            Destroy(partImage);
            return;
        }
        
        gravityGunPartImages[part] = partImage;

        // Update progress text if available
        UpdatePartsProgressText(collectedCount, totalCount);

        if (debugMode)
        {
            Debug.Log("Added gravity gun part to UI: " + part.partName);
        }
    }
    
    public void RemoveGravityGunPartFromUI(GravityGunPart part, int collectedCount, int totalCount)
    {
        if (gravityGunPartImages.ContainsKey(part))
        {
            Destroy(gravityGunPartImages[part]);
            gravityGunPartImages.Remove(part);
            
            // Update progress text
            UpdatePartsProgressText(collectedCount, totalCount);
            
            // Hide panel if no parts left
            if (gravityGunPartImages.Count == 0 && gravityGunPartsPanel != null)
            {
                gravityGunPartsPanel.gameObject.SetActive(false);
            }
            
            if (debugMode)
            {
                Debug.Log("Removed part from UI: " + part.partName);
            }
        }
    }
    
    private void UpdatePartsProgressText(int collected, int total)
    {
        if (partsProgressText != null)
        {
            partsProgressText.text = $"{collected}/{total} Parts Collected";
            
            // Change color based on progress
            if (collected >= total)
            {
                partsProgressText.color = Color.green;
            }
            else if (collected >= total / 2)
            {
                partsProgressText.color = Color.yellow;
            }
            else
            {
                partsProgressText.color = Color.white;
            }
        }
    }
    
    public void ClearAllGravityGunParts()
    {
        foreach (GameObject partImage in gravityGunPartImages.Values)
        {
            Destroy(partImage);
        }
        gravityGunPartImages.Clear();
        
        // Hide the panel
        if (gravityGunPartsPanel != null)
        {
            gravityGunPartsPanel.gameObject.SetActive(false);
        }
        
        // Clear progress text
        if (partsProgressText != null)
        {
            partsProgressText.text = "";
        }
        
        if (debugMode)
        {
            Debug.Log("Cleared all gravity gun parts from UI");
        }
    }

    // Additional method to update progress text without adding/removing parts
    public void UpdateGravityGunProgress(int collected, int total)
    {
        UpdatePartsProgressText(collected, total);
    }
}