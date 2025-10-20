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
    [SerializeField] private float spawnScale = 0.3f; // Adjusted to reasonable size
    
    [Header("UI Elements")]
    [SerializeField] private GameObject mixPrompt;
    [SerializeField] private GameObject missingItemsPrompt;
    [SerializeField] private TMP_Text statusText;
    
    private bool isMixing = false;

    private void Start()
    {
        HidePrompts();
        
        if (reactionLight != null)
        {
            reactionLight.enabled = false;
        }
        
        UpdateStatusText();
    }
    
    public void Interact()
    {
        if (isMixing) return;
        
        TryMixChemical();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HidePrompts();
        }
    }
    
    public void TryMixChemical()
    {
        if (isMixing) return;
        
        if (!CanMixChemical())
        {
            Debug.Log("Cannot mix chemical. Missing required items.");
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
        
        Debug.Log("Starting chemical mixing process...");
        
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            ChemicalInventory.Instance.RemoveChemical(chemicalReq.chemical, chemicalReq.amount);
        }
        
        if (reactionParticles != null)
        {
            reactionParticles.Play();
        }
        
        if (reactionLight != null)
        {
            reactionLight.enabled = true;
            reactionLight.color = gravityGunChemicalRecipe.reactionColor;
        }
        
        if (reactionSound != null)
        {
            AudioSource.PlayClipAtPoint(reactionSound, transform.position);
        }
        
        if (statusText != null)
        {
            statusText.text = "Mixing in progress...";
        }
        
        yield return new WaitForSeconds(gravityGunChemicalRecipe.mixTime);
        
        if (reactionLight != null)
        {
            reactionLight.enabled = false;
        }
        
        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, transform.position);
        }
        
        SpawnGravityGunChemical();
        
        Debug.Log("Chemical mixing complete!");
        
        UpdateStatusText();
        isMixing = false;
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
        chemical.SetActive(true);
        
        // Set the scale to a reasonable size (0.3 is good for most objects)
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
    
    private void HidePrompts()
    {
        if (mixPrompt != null) mixPrompt.SetActive(false);
        if (missingItemsPrompt != null) missingItemsPrompt.SetActive(false);
    }
    
    // Optional: Keep this for testing, remove when done
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnGravityGunChemical();
        }
    }
}