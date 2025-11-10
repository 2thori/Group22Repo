using UnityEngine;

public class GravityGunPartCollectable : MonoBehaviour, IInteractable
{
    [SerializeField] private GravityGunPart part;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectAudio;
    
    public GravityGunPart Part => part;
    
    void Start()
    {
        Debug.Log($"[DEBUG] Collectable Start: {gameObject.name}");
        Debug.Log($"[DEBUG] Part assigned: {part != null} - {(part != null ? part.partName : "NULL")}");
        Debug.Log($"[DEBUG] Has collider: {GetComponent<Collider>() != null}");
        if (GetComponent<Collider>() != null)
        {
            Debug.Log($"[DEBUG] Collider is trigger: {GetComponent<Collider>().isTrigger}");
        }
    }
    
    public void Interact()
    {
        Debug.Log($"[DEBUG] Interact() called on {gameObject.name}");
        CollectPart();
    }
    
    private void CollectPart()
    {
        Debug.Log($"[DEBUG] CollectPart() started for {gameObject.name}");
        
        if (part != null)
        {
            Debug.Log($"[DEBUG] Part found: {part.partName}");
            
            // Check if inventory exists
            bool inventoryExists = GravityGunPartsInventory.Instance != null;
            Debug.Log($"[DEBUG] Inventory exists: {inventoryExists}");
            
            if (inventoryExists)
            {
                Debug.Log("[DEBUG] Attempting to add part to inventory...");
                GravityGunPartsInventory.Instance.AddPart(part);
                
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, transform.rotation);
                    Debug.Log("[DEBUG] Collection effect spawned");
                }
                
                if (collectAudio != null)
                {
                    AudioSource.PlayClipAtPoint(collectAudio, transform.position);
                    Debug.Log("[DEBUG] Collection audio played");
                }
                
                gameObject.SetActive(false);
                Debug.Log($"[DEBUG] Part collected and object disabled: {part.partName}");
            }
            else
            {
                Debug.LogError("[DEBUG] GravityGunPartsInventory instance not found!");
                Debug.LogError("[DEBUG] Make sure there's a GravityGunPartsInventory in the scene");
            }
        }
        else
        {
            Debug.LogError("[DEBUG] Part not assigned in GravityGunPartCollectable on " + gameObject.name);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DEBUG] OnTriggerEnter with: {other.gameObject.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("[DEBUG] Player entered trigger, collecting part...");
            CollectPart();
        }
        else
        {
            Debug.Log($"[DEBUG] Not player - tag was: {other.tag}");
        }
    }
}