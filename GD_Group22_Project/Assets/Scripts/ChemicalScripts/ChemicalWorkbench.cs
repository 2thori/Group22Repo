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
    }
    
    private void EnsureCollider()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(5, 5, 5);
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            
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
            
            if (workbenchTitleText != null)
            {
                workbenchTitleText.text = workbenchName;
            }
        }
    }
    
    public void Interact()
    {
        if (isMixing) return;
        
        TryMixChemical();
    }
    
    // CHANGED FROM GetInteractText() TO GetInteractionText()
    public string GetInteractionText()
    {
        return InteractionText;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
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
            HidePrompts();
            HideInfoPanel();
        }
    }
    
    public void TryMixChemical()
    {
        if (isMixing) return;
        
        if (!CanMixChemical())
        {
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

        if (gravityGunChemicalRecipe == null)
        {
            Debug.LogError("Gravity Gun Chemical Recipe is not assigned!");
            return false;
        }
        
        // Check required chemicals
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            if (chemicalReq.chemical == null)
            {
                Debug.LogError("One of the required chemicals in the recipe is null!");
                return false;
            }

            if (!ChemicalInventory.Instance.HasChemical(chemicalReq.chemical, chemicalReq.amount))
            {
                return false;
            }
        }
        
        // Check required apparatus
        foreach (var apparatusReq in gravityGunChemicalRecipe.requiredApparatus)
        {
            if (apparatusReq == null)
            {
                Debug.LogError("One of the required apparatus in the recipe is null!");
                return false;
            }

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
        
        // Remove required items from inventory
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            ChemicalInventory.Instance.RemoveChemical(chemicalReq.chemical, chemicalReq.amount);
        }
        
        // Play visual effects
        if (reactionParticles != null) 
        {
            reactionParticles.Play();
        }
        
        if (reactionLight != null)
        {
            reactionLight.enabled = true;
            reactionLight.color = gravityGunChemicalRecipe.reactionColor;
        }
        
        // Play audio effects safely
        PlayAudioEffect(reactionSound);
        
        if (statusText != null) 
        {
            statusText.text = "Mixing in progress...";
        }
        
        // Wait for mixing time
        yield return new WaitForSeconds(gravityGunChemicalRecipe.mixTime);
        
        // Clean up effects
        if (reactionLight != null)
        {
            reactionLight.enabled = false;
        }
        
        PlayAudioEffect(successSound);
        
        // Spawn the result
        SpawnGravityGunChemical();
        
        InteractionText = completeText;
        UpdateStatusText();
        UpdateRequiredItemsText();
        
        StartCoroutine(ResetInteractionTextAfterDelay(3f));
        isMixing = false;
    }
    
    private void PlayAudioEffect(AudioClip clip)
    {
        if (clip != null)
        {
            // Create temporary GameObject for audio to avoid issues when object is destroyed
            GameObject audioGameObject = new GameObject("TempAudio");
            AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
            
            // Destroy the temporary object after the clip finishes
            Destroy(audioGameObject, clip.length);
        }
    }
    
    private IEnumerator ResetInteractionTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateInteractionText();
    }
    
    private void SpawnGravityGunChemical()
    {
        if (gravityGunChemicalPrefab == null)
        {
            Debug.LogError("Gravity Gun Chemical Prefab is not assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point is not assigned!");
            return;
        }

        GameObject chemical = Instantiate(gravityGunChemicalPrefab, spawnPoint.position, spawnPoint.rotation);
        chemical.transform.localScale = Vector3.one * spawnScale;
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
        if (requiredItemsText == null) return;
        
        if (gravityGunChemicalRecipe == null)
        {
            requiredItemsText.text = "Recipe not assigned!";
            return;
        }

        if (ChemicalInventory.Instance == null)
        {
            requiredItemsText.text = "Inventory not found!";
            return;
        }
        
        string itemsText = "Required Items:\n\n<b>Chemicals:</b>\n";
        
        // Chemicals section
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            if (chemicalReq.chemical == null) continue;
            
            bool hasChemical = ChemicalInventory.Instance.HasChemical(chemicalReq.chemical, chemicalReq.amount);
            string chemicalName = chemicalReq.chemical.chemicalName ?? "Unknown Chemical";
            string status = hasChemical ? "<color=green>✓</color>" : "<color=red>✗</color>";
            
            itemsText += $"{status} {chemicalName} x{chemicalReq.amount}\n";
        }
        
        itemsText += "\n<b>Apparatus:</b>\n";
        
        // Apparatus section
        foreach (var apparatusReq in gravityGunChemicalRecipe.requiredApparatus)
        {
            if (apparatusReq == null) continue;
            
            bool hasApparatus = ChemicalInventory.Instance.HasApparatus(apparatusReq);
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
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            UpdateRequiredItemsText();
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
    
    // Update title when object becomes enabled
    private void OnEnable()
    {
        if (workbenchTitleText != null && !string.IsNullOrEmpty(workbenchName))
        {
            workbenchTitleText.text = workbenchName;
        }
    }
}