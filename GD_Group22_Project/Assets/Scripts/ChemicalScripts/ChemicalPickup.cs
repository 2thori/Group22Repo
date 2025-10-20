using UnityEngine;

public class ChemicalPickup : MonoBehaviour, IInteractable
{
    [Header("Chemical Settings")]
    [SerializeField] private ChemicalItem chemical;
    [SerializeField] private int amount = 1;
    [SerializeField] private string interactText = "Press E to collect ";
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem collectParticles;
    [SerializeField] private AudioClip collectSound;
    
    private Renderer liquidRenderer;
    private Collider pickupCollider;
    private bool isCollected = false;

    private void Start()
    {
        pickupCollider = GetComponent<Collider>();
        
        // Set up liquid color if this chemical has one
        if (chemical != null && chemical.liquidColor != Color.white)
        {
            liquidRenderer = GetComponentInChildren<Renderer>();
            if (liquidRenderer != null)
            {
                liquidRenderer.material.color = chemical.liquidColor;
                liquidRenderer.material.SetColor("_EmissionColor", chemical.liquidColor);
            }
        }
        
        interactText += chemical != null ? chemical.chemicalName : "Chemical";
    }
    
    public void Interact()
    {
        if (isCollected) return;
        
        CollectChemical();
    }
    
    private void CollectChemical()
    {
        if (ChemicalInventory.Instance == null)
        {
            Debug.LogError("ChemicalInventory instance not found!");
            return;
        }
        
        // Add to inventory
        ChemicalInventory.Instance.AddChemical(chemical, amount);
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
        if (liquidRenderer != null) liquidRenderer.enabled = false;
        if (pickupCollider != null) pickupCollider.enabled = false;
        
        // Hide all child renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        Debug.Log($"Collected {amount} {chemical.chemicalName}");
        
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