using UnityEngine;

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
        interactText += apparatus != null ? apparatus.apparatusName : "Apparatus";
    }
    
    public void Interact()
    {
        if (isCollected) return;
        
        CollectApparatus();
    }
    
    private void CollectApparatus()
    {
        if (ChemicalInventory.Instance == null)
        {
            Debug.LogError("ChemicalInventory instance not found!");
            return;
        }
        
        // Add to inventory
        ChemicalInventory.Instance.AddApparatus(apparatus);
        isCollected = true;
        
        // Play effects
        if (collectParticles != null)
        {
            collectParticles.Play();
        }
        
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Immediately hide the object
        if (apparatusRenderer != null) apparatusRenderer.enabled = false;
        if (pickupCollider != null) pickupCollider.enabled = false;
        
        // Hide all child renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        Debug.Log($"Collected {apparatus.apparatusName}");
        
        // Destroy after effects finish
        Destroy(gameObject, 2f);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            Debug.Log(interactText);
        }
    }
}