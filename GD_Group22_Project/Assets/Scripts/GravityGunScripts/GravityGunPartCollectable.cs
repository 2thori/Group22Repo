using UnityEngine;

public class GravityGunPartCollectable : MonoBehaviour, IInteractable
{
    [SerializeField] private GravityGunPart part;
    [SerializeField] private GameObject collectEffect;
    
    public void Interact()
    {
        CollectPart();
    }
    
    private void CollectPart()
    {
        if (part != null)
        {
            if (GravityGunPartsInventory.Instance != null)
            {
                GravityGunPartsInventory.Instance.AddPart(part);
                
                // Play collection effect if assigned
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, transform.rotation);
                }
                
                // Disable the object instead of destroying it
                gameObject.SetActive(false);
                
                Debug.Log("Collected part: " + part.partName);
            }
            else
            {
                Debug.LogError("GravityGunPartsInventory instance not found!");
            }
        }
        else
        {
            Debug.LogError("Part not assigned in GravityGunPartCollectable on " + gameObject.name);
        }
    }
    
    // Optional: Keep the trigger-based collection as well
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectPart();
        }
    }
}