using UnityEngine;
using System.Collections;

public class WinObject : MonoBehaviour, IInteractable
{
    public enum WinObjectType { Object1, Object2 }
    
    [Header("Win Object Settings")]
    public WinObjectType objectType;
    public string objectName = "Win Object";
    
    [Header("Interaction Settings")]
    [SerializeField] private string interactionText = "Press E to Collect";
    [SerializeField] private string alreadyInteractedText = "Already collected";
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private Material collectedMaterial; // Optional: change material when collected
    
    private bool hasBeenInteracted = false;
    private Renderer objectRenderer;
    private Material originalMaterial;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
    }

    public void Interact()
    {
        if (hasBeenInteracted) 
        {
            Debug.Log($"{objectName} already collected");
            return;
        }

        hasBeenInteracted = true;
        Debug.Log($"Collecting {objectName}");

        PlayCollectionEffects();

        // Change appearance instead of disappearing
        if (collectedMaterial != null && objectRenderer != null)
        {
            objectRenderer.material = collectedMaterial;
        }

        // Notify win condition manager
        if (WinConditionManager.Instance != null)
        {
            WinConditionManager.Instance.RegisterWinObjectInteraction(objectType);
        }

        Debug.Log($"SUCCESS: {objectName} collected (but not disappeared)");
    }

    private void PlayCollectionEffects()
    {
        if (collectEffect != null)
        {
            collectEffect.Play();
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }

    public string GetInteractionText()
    {
        return hasBeenInteracted ? alreadyInteractedText : interactionText;
    }

    public bool CanInteract()
    {
        return !hasBeenInteracted;
    }

    // Reset the object (for testing)
    public void ResetObject()
    {
        hasBeenInteracted = false;
        if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }
    }

    // Visual feedback in editor
    private void OnDrawGizmos()
    {
        if (hasBeenInteracted) return;

        Gizmos.color = objectType == WinObjectType.Object1 ? Color.cyan : Color.magenta;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.5f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawIcon(transform.position + Vector3.up * 2, "d_Favorite@2x", true);
    }
}