using UnityEngine;
using System.Collections;
using TMPro;

public class ChemicalWorkbench : MonoBehaviour, IInteractable
{
    [Header("Recipe Requirements")]
    [SerializeField] private ChemicalRecipe gravityGunChemicalRecipe;
    
    [Header("Workbench Visuals")]
    [SerializeField] private ParticleSystem reactionParticles;
    [SerializeField] private Light reactionLight;
    [SerializeField] private AudioClip reactionSound;
    [SerializeField] private AudioClip successSound;
    
    [Header("Spawn Settings")]
    [SerializeField] private GameObject gravityGunChemicalPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnScale = 0.3f;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject mixPrompt;
    [SerializeField] private GameObject missingItemsPrompt;
    [SerializeField] private TMP_Text statusText;

    [Header("Info Panel UI")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text workbenchTitleText;
    [SerializeField] private TMP_Text requiredItemsText;
    [SerializeField] private string workbenchName = "Chemical Workbench";

    [Header("Interaction Text")]
    [SerializeField] private string readyText = "Press E to Mix Chemical";
    [SerializeField] private string missingItemsText = "Missing Ingredients";
    [SerializeField] private string mixingText = "Mixing in progress...";
    [SerializeField] private string completeText = "Mixing Complete!";
    
    private bool isMixing = false;
    private bool playerInRange = false;

    // For interaction system
    public string InteractionText { get; private set; }

    private void Start()
    {
        EnsureCollider();
        HidePrompts();
        
        if (reactionLight != null)
        {
            reactionLight.enabled = false;
        }
        
        InitializeInfoPanel();
        UpdateStatusText();
        UpdateInteractionText();
        UpdateRequiredItemsText();
        
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
    
    private void InitializeInfoPanel()
    {
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
        Debug.Log($"Items Text: {(requiredItemsText == null ? "NOT ASSIGNED" : "Assigned")}");
        
        if (workbenchTitleText != null)
        {
            Debug.Log($"Current Title: '{workbenchTitleText.text}'");
            Debug.Log($"Expected Title: '{workbenchName}'");
        }
        
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Position: {transform.position}");
        Debug.Log("=== End Debug ===");
    }
    
    public void Interact()
    {
        if (isMixing) return;
        
        TryMixChemical();
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
            
            if (CanMixChemical())
            {
                ShowMixPrompt();
            }
            else
            {
                ShowMissingItemsPrompt();
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
    
    public void TryMixChemical()
    {
        if (isMixing) return;
        
        if (!CanMixChemical())
        {
            Debug.Log("Cannot mix chemical. Missing required items.");
            UpdateInteractionText();
            return;
        }
        
        StartCoroutine(MixChemicalProcess());
    }
    
    private bool CanMixChemical()
    {
        if (ChemicalInventory.Instance == null)
        {
            Debug.LogError("ChemicalInventory instance not found!");
            return false;
        }
        
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            if (!ChemicalInventory.Instance.HasChemical(chemicalReq.chemical, chemicalReq.amount))
            {
                return false;
            }
        }
        
        foreach (var apparatusReq in gravityGunChemicalRecipe.requiredApparatus)
        {
            if (!ChemicalInventory.Instance.HasApparatus(apparatusReq))
            {
                return false;
            }
        }
        
        return true;
    }
    
    private IEnumerator MixChemicalProcess()
    {
        isMixing = true;
        HidePrompts();
        
        InteractionText = mixingText;
        
        Debug.Log("Starting chemical mixing process...");
        
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            ChemicalInventory.Instance.RemoveChemical(chemicalReq.chemical, chemicalReq.amount);
        }
        
        if (reactionParticles != null) reactionParticles.Play();
        if (reactionLight != null)
        {
            reactionLight.enabled = true;
            reactionLight.color = gravityGunChemicalRecipe.reactionColor;
        }
        if (reactionSound != null) AudioSource.PlayClipAtPoint(reactionSound, transform.position);
        if (statusText != null) statusText.text = "Mixing in progress...";
        
        yield return new WaitForSeconds(gravityGunChemicalRecipe.mixTime);
        
        if (reactionLight != null) reactionLight.enabled = false;
        if (successSound != null) AudioSource.PlayClipAtPoint(successSound, transform.position);
        
        SpawnGravityGunChemical();
        
        Debug.Log("Chemical mixing complete!");
        
        InteractionText = completeText;
        UpdateStatusText();
        UpdateRequiredItemsText();
        
        StartCoroutine(ResetInteractionTextAfterDelay(3f));
        isMixing = false;
    }
    
    private IEnumerator ResetInteractionTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateInteractionText();
    }
    
    private void SpawnGravityGunChemical()
    {
        if (gravityGunChemicalPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Gravity Gun Chemical Prefab or Spawn Point is not assigned!");
            return;
        }

        GameObject chemical = Instantiate(gravityGunChemicalPrefab, spawnPoint.position, spawnPoint.rotation);
        chemical.SetActive(true);
        chemical.transform.localScale = Vector3.one * spawnScale;
        
        Debug.Log($"Spawned gravity gun chemical at scale: {spawnScale}");
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            if (CanMixChemical())
            {
                statusText.text = "Ready to mix";
                statusText.color = Color.green;
            }
            else
            {
                statusText.text = "Missing items";
                statusText.color = Color.red;
            }
        }
    }
    
    private void UpdateInteractionText()
    {
        if (isMixing) return;
        
        if (CanMixChemical())
        {
            InteractionText = readyText;
        }
        else
        {
            InteractionText = missingItemsText;
        }
    }
    
    private void UpdateRequiredItemsText()
    {
        if (requiredItemsText == null) 
        {
            Debug.LogError("RequiredItemsText is not assigned!");
            return;
        }
        
        string itemsText = "Required Items:\n\n<b>Chemicals:</b>\n";
        
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            if (chemicalReq.chemical == null) continue;
            
            bool hasChemical = ChemicalInventory.Instance != null && 
                              ChemicalInventory.Instance.HasChemical(chemicalReq.chemical, chemicalReq.amount);
            
            string chemicalName = chemicalReq.chemical.chemicalName ?? "Unknown Chemical";
            string status = hasChemical ? "<color=green>✓</color>" : "<color=red>✗</color>";
            
            itemsText += $"{status} {chemicalName} x{chemicalReq.amount}\n";
        }
        
        itemsText += "\n<b>Apparatus:</b>\n";
        foreach (var apparatusReq in gravityGunChemicalRecipe.requiredApparatus)
        {
            if (apparatusReq == null) continue;
            
            bool hasApparatus = ChemicalInventory.Instance != null && 
                               ChemicalInventory.Instance.HasApparatus(apparatusReq);
            
            string apparatusName = apparatusReq.apparatusName ?? "Unknown Apparatus";
            string status = hasApparatus ? "<color=green>✓</color>" : "<color=red>✗</color>";
            
            itemsText += $"{status} {apparatusName}\n";
        }
        
        requiredItemsText.text = itemsText;
    }
    
    private void ShowMixPrompt()
    {
        if (mixPrompt != null) mixPrompt.SetActive(true);
        if (missingItemsPrompt != null) missingItemsPrompt.SetActive(false);
    }
    
    private void ShowMissingItemsPrompt()
    {
        if (missingItemsPrompt != null) missingItemsPrompt.SetActive(true);
        if (mixPrompt != null) mixPrompt.SetActive(false);
    }
    
    private void ShowInfoPanel()
    {
        Debug.Log($"{workbenchName}: ShowInfoPanel called");
        
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            UpdateRequiredItemsText();
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
        if (mixPrompt != null) mixPrompt.SetActive(false);
        if (missingItemsPrompt != null) missingItemsPrompt.SetActive(false);
    }
    
    public void RefreshUI()
    {
        UpdateInteractionText();
        UpdateRequiredItemsText();
        UpdateStatusText();
        
        if (playerInRange)
        {
            if (CanMixChemical())
            {
                ShowMixPrompt();
            }
            else
            {
                ShowMissingItemsPrompt();
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
    
    // Optional: Testing
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnGravityGunChemical();
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            UpdateInteractionText();
            UpdateRequiredItemsText();
        }
    }
}