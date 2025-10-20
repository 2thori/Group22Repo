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
        
        // Debug log to check if everything is assigned
        Debug.Log($"ChemicalWorkbench Start - Prefab: {gravityGunChemicalPrefab != null}, SpawnPoint: {spawnPoint != null}, Recipe: {gravityGunChemicalRecipe != null}");
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
        
        // Check if we have all required chemicals
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            if (!ChemicalInventory.Instance.HasChemical(chemicalReq.chemical, chemicalReq.amount))
            {
                return false;
            }
        }
        
        // Check if we have all required apparatus
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
        
        // Consume required chemicals
        foreach (var chemicalReq in gravityGunChemicalRecipe.requiredChemicals)
        {
            ChemicalInventory.Instance.RemoveChemical(chemicalReq.chemical, chemicalReq.amount);
        }
        
        // Play reaction effects
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
        
        // Update status during mixing
        if (statusText != null)
        {
            statusText.text = "Mixing in progress...";
        }
        
        // Wait for reaction to complete
        yield return new WaitForSeconds(gravityGunChemicalRecipe.mixTime);
        
        // Complete the reaction
        if (reactionLight != null)
        {
            reactionLight.enabled = false;
        }
        
        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, transform.position);
        }
        
        // Spawn the gravity gun chemical
        SpawnGravityGunChemical();
        
        Debug.Log("Chemical mixing complete!");
        
        UpdateStatusText();
        isMixing = false;
    }
    
    private void SpawnGravityGunChemical()
    {
        Debug.Log("=== SPAWNING GRAVITY GUN CHEMICAL ===");
        
        // Check if prefab is assigned
        if (gravityGunChemicalPrefab == null)
        {
            Debug.LogError("❌ CRITICAL: Gravity Gun Chemical Prefab is NOT assigned in inspector!");
            return;
        }
        else
        {
            Debug.Log("✅ Prefab is assigned: " + gravityGunChemicalPrefab.name);
        }

        // Check if spawn point is assigned
        if (spawnPoint == null)
        {
            Debug.LogError("❌ CRITICAL: Spawn Point is NOT assigned in inspector!");
            return;
        }
        else
        {
            Debug.Log("✅ Spawn Point is assigned at position: " + spawnPoint.position);
        }

        // Instantiate the object
        Debug.Log("Instantiating prefab...");
        GameObject chemical = Instantiate(gravityGunChemicalPrefab, spawnPoint.position, spawnPoint.rotation);
        
        if (chemical == null)
        {
            Debug.LogError("❌ INSTANTIATE FAILED: Object is null after Instantiate!");
            return;
        }
        else
        {
            Debug.Log("✅ Instantiate successful: " + chemical.name);
        }

        // Make sure the object is active
        chemical.SetActive(true);
        Debug.Log("✅ Object set to active");

        // Setup all required components
        SetupSpawnedObject(chemical);
        
        Debug.Log("🎉 SPAWN COMPLETE: Gravity gun chemical should be visible now!");
    }
    
    private void SetupSpawnedObject(GameObject chemical)
    {
        // Check for renderer
        Renderer renderer = chemical.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            Debug.Log("✅ Renderer found and enabled");
        }
        else
        {
            Debug.LogWarning("⚠️ No Renderer component found on spawned object");
        }

        // Check for collider
        Collider collider = chemical.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
            Debug.Log("✅ Collider found and enabled");
        }
        else
        {
            Debug.LogWarning("⚠️ No Collider found, adding BoxCollider");
            chemical.AddComponent<BoxCollider>();
        }

        // Check for Rigidbody
        Rigidbody rb = chemical.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            Debug.Log("✅ Rigidbody found and set to non-kinematic");
        }
        else
        {
            Debug.LogWarning("⚠️ No Rigidbody found, adding one");
            Rigidbody newRb = chemical.AddComponent<Rigidbody>();
            newRb.isKinematic = false;
        }

        // Check for GravityGunPartCollectable script
        GravityGunPartCollectable collectable = chemical.GetComponent<GravityGunPartCollectable>();
        if (collectable != null)
        {
            Debug.Log("✅ GravityGunPartCollectable found");
            
            // Use the public property to check if part is assigned
            if (collectable.Part == null)
            {
                Debug.LogError("❌ GravityGunPartCollectable has no part assigned!");
            }
            else
            {
                Debug.Log("✅ Part assigned: " + collectable.Part.partName);
            }
        }
        else
        {
            Debug.LogError("❌ No GravityGunPartCollectable found on the chemical prefab!");
        }
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
    
    // TEST METHOD: Add this to test spawning without the mixing process
    private void Update()
    {
        // Test spawn with T key (remove this method once spawning works)
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("=== MANUAL SPAWN TEST ===");
            SpawnGravityGunChemical();
        }
    }
}