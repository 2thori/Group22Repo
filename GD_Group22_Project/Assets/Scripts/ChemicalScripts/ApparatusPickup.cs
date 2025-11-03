using UnityEngine;

public class ApparatusPickup : MonoBehaviour, IInteractable
{
    [Header("Apparatus Settings")]
    [SerializeField] private ApparatusItem apparatus;
    [SerializeField] private string interactText = "Press E to collect ";
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem collectParticles;
    [SerializeField] private AudioClip collectSound;
    
    private Renderer apparatusRenderer;
    private Collider pickupCollider;
    private bool isCollected = false;

    private void Start()
    {
        apparatusRenderer = GetComponent<Renderer>();
        pickupCollider = GetComponent<Collider>();
        
        // Safe null check for apparatus
        if (apparatus != null)
        {
            interactText += apparatus.apparatusName;
        }
        else
        {
            interactText += "Apparatus";
            Debug.LogError("ApparatusItem is not assigned in the inspector!", this);
        }
    }
    
    public void Interact()
    {
        if (isCollected) return;
        
        CollectApparatus();
    }
    
    private void CollectApparatus()
    {
        // Safe null check for ChemicalInventory
        if (ChemicalInventory.Instance == null)
        {
            Debug.LogError("ChemicalInventory instance not found! Make sure it exists in the scene.", this);
            return;
        }
        
        // Safe null check for apparatus
        if (apparatus == null)
        {
            Debug.LogError("Cannot collect null apparatus!", this);
            return;
        }
        
        // Add to inventory
        ChemicalInventory.Instance.AddApparatus(apparatus);
        isCollected = true;
        
        // Play effects
        PlayCollectionEffects();
        
        // Hide the object immediately
        HideObject();
        
        Debug.Log($"Collected {apparatus.apparatusName}");
        
        // Destroy after effects finish
        Destroy(gameObject, 2f);
    }
    
    private void PlayCollectionEffects()
    {
        // Handle particle effects
        if (collectParticles != null)
        {
            // Detach particles so they don't get destroyed with the object
            collectParticles.transform.SetParent(null);
            collectParticles.Play();
            
            // Destroy particles after they finish
            Destroy(collectParticles.gameObject, collectParticles.main.duration);
        }
        
        // Handle sound
        if (collectSound != null)
        {
            // Create a temporary GameObject for playing the sound
            GameObject soundGameObject = new GameObject("TempAudio");
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            audioSource.clip = collectSound;
            audioSource.Play();
            
            // Destroy the temporary object after the clip finishes
            Destroy(soundGameObject, collectSound.length);
        }
    }
    
    private void HideObject()
    {
        // Disable renderer and collider
        if (apparatusRenderer != null) 
            apparatusRenderer.enabled = false;
        
        if (pickupCollider != null) 
            pickupCollider.enabled = false;
        
        // Hide all child renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        // Also disable all colliders in children
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }
    
    public string GetInteractText()
    {
        return interactText;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            // Optional: Show UI prompt here
            Debug.Log(interactText);
        }
    }
    
    // Optional: Add this for better build debugging
    private void OnDestroy()
    {
        if (isCollected)
        {
            Debug.Log($"Apparatus pickup destroyed: {apparatus?.apparatusName}");
        }
    }
}