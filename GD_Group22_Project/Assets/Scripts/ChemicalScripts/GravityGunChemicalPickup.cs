using UnityEngine;

public class GravityGunChemicalPickup : MonoBehaviour, IInteractable
{
    [Header("Chemical Settings")]
    [SerializeField] private string interactText = "Press E to collect Gravity Gun Chemical";
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem glowParticles;
    [SerializeField] private AudioClip collectSound;
    
    private bool isCollected = false;

    private void Start()
    {
        // Start glowing effect
        if (glowParticles != null && !glowParticles.isPlaying)
        {
            glowParticles.Play();
        }
    }
    
    public void Interact()
    {
        if (isCollected) return;
        
        CollectChemical();
    }
    
    private void CollectChemical()
    {
        // Add to gravity gun parts inventory or direct to workbench
        if (GravityGunPartsInventory.Instance != null)
        {
            // Assuming you have a method to add chemicals to the gravity gun inventory
            // GravityGunPartsInventory.Instance.AddChemicalPart(this);
        }
        
        isCollected = true;
        
        // Play effects
        if (glowParticles != null)
        {
            glowParticles.Stop();
        }
        
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Add to player's inventory or progress
        Debug.Log("Collected Gravity Gun Chemical! This can be used to assemble the gravity gun.");
        
        // Disable and destroy
        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;
        
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