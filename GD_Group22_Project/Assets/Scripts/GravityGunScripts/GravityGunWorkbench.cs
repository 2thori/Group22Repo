using UnityEngine;
using TMPro;

public class GravityGunWorkbench : MonoBehaviour, IInteractable
{
    [Header("Workbench Settings")]
    [SerializeField] private Transform gravityGunPivot;
    [SerializeField] private Vector3 assembledPosition = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 disassembledPosition = new Vector3(0, 0, 0);
    [SerializeField] private float assemblySpeed = 2f;

    [Header("Assembly Requirements")]
    [SerializeField] private bool requiresParts = true;
    [SerializeField] private GravityGunPart[] requiredParts;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject assemblyPrompt;
    [SerializeField] private GameObject missingPartsPrompt;

    [Header("Info Panel UI")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text workbenchTitleText;
    [SerializeField] private TMP_Text requiredPartsText;
    [SerializeField] private string workbenchName = "Gravity Gun Workbench";

    [Header("Gravity Gun Prefab")]
    [SerializeField] private GameObject gravityGunPrefab;
    [SerializeField] private Transform gunSpawnPoint;

    [Header("Interaction Text")]
    [SerializeField] private string readyToAssembleText = "Press E to Assemble Gravity Gun";
    [SerializeField] private string readyToDisassembleText = "Press E to Disassemble Gravity Gun";
    [SerializeField] private string missingPartsText = "Missing Parts";
    [SerializeField] private string assemblingText = "Assembling...";
    [SerializeField] private string disassemblingText = "Disassembling...";

    [Header("Sound Effects")] // Added sound effects header
    [SerializeField] private AudioClip assemblingSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private float soundVolume = 1f;

    private bool isGunAssembled = false;
    private bool isAnimating = false;
    private Vector3 targetPosition;
    private GameObject spawnedGun;
    private bool playerInRange = false;
    private AudioSource audioSource; // Reference to AudioSource component

    // For interaction system
    public string InteractionText { get; private set; }

    private void Start()
    {
        EnsureCollider();
        
        // Set up AudioSource component
        SetupAudioSource();
        
        targetPosition = disassembledPosition;
        gravityGunPivot.localPosition = targetPosition;
        
        InitializeUI();
        UpdateRequiredPartsText();
        UpdateInteractionText();
        
        DebugWorkbenchSetup();
    }

    private void EnsureCollider()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(5, 5, 5);
            Debug.Log($"{workbenchName}: Added large trigger collider");
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log($"{workbenchName}: Set existing collider to trigger");
            
            BoxCollider boxColl = collider as BoxCollider;
            if (boxColl != null)
            {
                boxColl.size = new Vector3(5, 5, 5);
            }
        }
    }
    
    private void SetupAudioSource()
    {
        // Try to get existing AudioSource, or add one if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource settings
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.volume = soundVolume;
        audioSource.playOnAwake = false;
    }
    
    private void InitializeUI()
    {
        if (assemblyPrompt != null) assemblyPrompt.SetActive(false);
        if (missingPartsPrompt != null) missingPartsPrompt.SetActive(false);
        
        if (infoPanel != null) 
        {
            infoPanel.SetActive(false);
            
            // Force set the title to ensure it's correct
            if (workbenchTitleText != null)
            {
                workbenchTitleText.text = workbenchName;
            }
            else
            {
                Debug.LogError($"{workbenchName}: WorkbenchTitleText is not assigned!");
            }
        }
        else
        {
            Debug.LogError($"{workbenchName}: InfoPanel is not assigned!");
        }
    }
    
    private void DebugWorkbenchSetup()
    {
        Debug.Log($"=== {workbenchName} Setup Debug ===");
        Debug.Log($"Info Panel: {(infoPanel == null ? "NOT ASSIGNED" : "Assigned")}");
        Debug.Log($"Title Text: {(workbenchTitleText == null ? "NOT ASSIGNED" : "Assigned")}");
        Debug.Log($"Parts Text: {(requiredPartsText == null ? "NOT ASSIGNED" : "Assigned")}");
        
        if (workbenchTitleText != null)
        {
            Debug.Log($"Current Title: '{workbenchTitleText.text}'");
            Debug.Log($"Expected Title: '{workbenchName}'");
        }
        
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Position: {transform.position}");
        Debug.Log("=== End Debug ===");
    }

    private void Update()
    {
        if (isAnimating)
        {
            gravityGunPivot.localPosition = Vector3.MoveTowards(
                gravityGunPivot.localPosition,
                targetPosition,
                assemblySpeed * Time.deltaTime
            );

            if (Vector3.Distance(gravityGunPivot.localPosition, targetPosition) < 0.01f)
            {
                gravityGunPivot.localPosition = targetPosition;
                isAnimating = false;
                
                if (isGunAssembled)
                {
                    OnGravityGunAssembled();
                }
                else
                {
                    OnGravityGunDisassembled();
                }
                
                UpdateInteractionText();
                
                if (playerInRange)
                {
                    UpdateRequiredPartsText();
                }
            }
        }
    }

    public void Interact()
    {
        TryAssembleGun();
    }
    
    public string GetInteractionText()
    {
        return InteractionText;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{workbenchName}: OnTriggerEnter with {other.name}");
        
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"{workbenchName}: Player entered trigger area");
            
            if (CanAssembleGun())
            {
                ShowAssemblyPrompt();
            }
            else
            {
                ShowMissingPartsPrompt();
            }
            
            ShowInfoPanel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log($"{workbenchName}: Player left trigger area");
            HidePrompts();
            HideInfoPanel();
        }
    }

    public void TryAssembleGun()
    {
        if (isAnimating) return;

        if (!CanAssembleGun())
        {
            Debug.Log("Cannot assemble gravity gun. Missing required parts.");
            UpdateInteractionText();
            return;
        }

        InteractionText = isGunAssembled ? disassemblingText : assemblingText;

        // Play assembling sound effect
        PlayAssemblingSound();

        isGunAssembled = !isGunAssembled;
        targetPosition = isGunAssembled ? assembledPosition : disassembledPosition;
        isAnimating = true;

        if (isGunAssembled && requiresParts)
        {
            ConsumeRequiredParts();
        }
        
        HidePrompts();
        UpdateRequiredPartsText();
    }

    private bool CanAssembleGun()
    {
        if (!requiresParts) return true;
        
        if (GravityGunPartsInventory.Instance == null)
        {
            Debug.LogError("GravityGunPartsInventory instance not found!");
            return false;
        }

        foreach (GravityGunPart part in requiredParts)
        {
            if (!GravityGunPartsInventory.Instance.HasPart(part))
            {
                return false;
            }
        }

        return true;
    }

    private void ConsumeRequiredParts()
    {
        if (GravityGunPartsInventory.Instance == null) return;

        foreach (GravityGunPart part in requiredParts)
        {
            GravityGunPartsInventory.Instance.RemovePart(part);
        }

        Debug.Log("Consumed all required parts for gravity gun assembly.");
    }

    private void OnGravityGunAssembled()
    {
        Debug.Log("Gravity gun has been assembled and is ready for use!");
        
        // Play success sound effect
        PlaySuccessSound();
        
        if (gravityGunPrefab != null && gunSpawnPoint != null)
        {
            if (spawnedGun != null)
            {
                Destroy(spawnedGun);
            }
            
            spawnedGun = Instantiate(gravityGunPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
            Debug.Log("Gravity gun spawned successfully!");
            
            SetupSpawnedGun(spawnedGun);
        }
        else
        {
            Debug.LogError("Gravity gun prefab or spawn point is not assigned in the inspector!");
        }
    }

    private void OnGravityGunDisassembled()
    {
        Debug.Log("Gravity gun has been disassembled.");
        
        // Play success sound effect for disassembly too
        PlaySuccessSound();
        
        if (spawnedGun != null)
        {
            Destroy(spawnedGun);
            spawnedGun = null;
        }
    }

    private void SetupSpawnedGun(GameObject gunObject)
    {
        if (gunObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gunObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        if (gunObject.GetComponent<Collider>() == null)
        {
            gunObject.AddComponent<BoxCollider>();
        }

        if (gunObject.GetComponent<GravityGunPickup>() == null)
        {
            gunObject.AddComponent<GravityGunPickup>();
        }

        if (gunObject.GetComponent<GravityGun>() == null)
        {
            Debug.LogWarning("Spawned gravity gun prefab is missing the GravityGun component!");
        }
    }

    // Sound effect methods
    private void PlayAssemblingSound()
    {
        if (assemblingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(assemblingSound, soundVolume);
        }
        else if (assemblingSound == null)
        {
            Debug.LogWarning("Assembling sound is not assigned!");
        }
    }

    private void PlaySuccessSound()
    {
        if (successSound != null)
        {
            // Use PlayClipAtPoint for success sound to ensure it plays even if the workbench is disabled
            AudioSource.PlayClipAtPoint(successSound, transform.position, soundVolume);
        }
        else if (successSound == null)
        {
            Debug.LogWarning("Success sound is not assigned!");
        }
    }
    
    private void UpdateInteractionText()
    {
        if (isAnimating) return;
        
        if (!CanAssembleGun())
        {
            InteractionText = missingPartsText;
        }
        else
        {
            InteractionText = isGunAssembled ? readyToDisassembleText : readyToAssembleText;
        }
    }
    
    private void UpdateRequiredPartsText()
    {
        if (requiredPartsText == null) 
        {
            Debug.LogError("RequiredPartsText is not assigned!");
            return;
        }
        
        if (!requiresParts)
        {
            requiredPartsText.text = "No parts required";
            return;
        }
        
        string partsText = "Required Parts:\n";
        
        foreach (GravityGunPart part in requiredParts)
        {
            if (part == null) continue;
            
            bool hasPart = GravityGunPartsInventory.Instance != null && 
                          GravityGunPartsInventory.Instance.HasPart(part);
            
            string partName = part.partName ?? "Unknown Part";
            string status = hasPart ? "<color=green>✓</color>" : "<color=red>✗</color>";
            
            partsText += $"{status} {partName}\n";
        }
        
        requiredPartsText.text = partsText;
    }

    private void ShowAssemblyPrompt()
    {
        if (assemblyPrompt != null)
        {
            assemblyPrompt.SetActive(true);
        }
        
        if (missingPartsPrompt != null)
        {
            missingPartsPrompt.SetActive(false);
        }
    }

    private void ShowMissingPartsPrompt()
    {
        if (missingPartsPrompt != null)
        {
            missingPartsPrompt.SetActive(true);
        }
        
        if (assemblyPrompt != null)
        {
            assemblyPrompt.SetActive(false);
        }
    }
    
    private void ShowInfoPanel()
    {
        Debug.Log($"{workbenchName}: ShowInfoPanel called");
        
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            UpdateRequiredPartsText();
            Debug.Log($"{workbenchName}: Info panel should now be visible");
        }
        else
        {
            Debug.LogError($"{workbenchName}: InfoPanel is not assigned!");
        }
    }
    
    private void HideInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void HidePrompts()
    {
        if (assemblyPrompt != null) assemblyPrompt.SetActive(false);
        if (missingPartsPrompt != null) missingPartsPrompt.SetActive(false);
    }

    public void RefreshUI()
    {
        UpdateInteractionText();
        UpdateRequiredPartsText();
        
        if (playerInRange)
        {
            if (CanAssembleGun())
            {
                ShowAssemblyPrompt();
            }
            else
            {
                ShowMissingPartsPrompt();
            }
        }
    }
    
    // Editor-only: Update title when values change in inspector
    private void OnValidate()
    {
        #if UNITY_EDITOR
        if (workbenchTitleText != null && !string.IsNullOrEmpty(workbenchName))
        {
            workbenchTitleText.text = workbenchName;
        }
        #endif
    }
    
    // Update title when object becomes enabled
    private void OnEnable()
    {
        if (workbenchTitleText != null && !string.IsNullOrEmpty(workbenchName))
        {
            workbenchTitleText.text = workbenchName;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + assembledPosition, Vector3.one * 0.3f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + disassembledPosition, Vector3.one * 0.3f);
        
        if (gunSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(gunSpawnPoint.position, 0.2f);
        }
        
        Gizmos.color = Color.yellow;
        Collider collider = GetComponent<Collider>();
        if (collider != null && collider.isTrigger)
        {
            if (collider is BoxCollider)
            {
                BoxCollider boxColl = (BoxCollider)collider;
                Gizmos.DrawWireCube(transform.position + boxColl.center, boxColl.size);
            }
        }
    }
}