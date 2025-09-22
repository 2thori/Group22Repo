using UnityEngine;

public class GravityGunPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Press E to pickup Gravity Gun";
    
    public void Interact()
    {
        PickupGravityGun();
    }
    
    private void PickupGravityGun()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Add the gravity gun to the player
            GravityGun gravityGun = player.GetComponent<GravityGun>();
            if (gravityGun == null)
            {
                gravityGun = player.AddComponent<GravityGun>();
            }
            
            // Enable the gravity gun
            gravityGun.enabled = true;
            
            // Attach to player's hand or camera
            Transform playerCamera = player.GetComponentInChildren<Camera>().transform;
            transform.SetParent(playerCamera);
            transform.localPosition = new Vector3(0.5f, -0.5f, 1f);
            transform.localRotation = Quaternion.identity;
            
            // Disable physics
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // Disable the collider
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            Debug.Log("Picked up the gravity gun!");
        }
    }
    
    // Show interaction text when player looks at the gun
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // You might want to implement a UI system to show interaction prompts
            Debug.Log(interactText);
        }
    }
}