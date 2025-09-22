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
        // Find the player more safely
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure your player has the 'Player' tag.");
            return;
        }

        // Check if player already has a gravity gun
        if (player.GetComponent<GravityGun>() != null)
        {
            Debug.Log("Player already has a gravity gun!");
            return;
        }

        // Add the gravity gun component to the player
        GravityGun gravityGun = player.AddComponent<GravityGun>();
        gravityGun.enabled = true;
        
        // Attach to player's camera
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            transform.SetParent(playerCamera.transform);
            transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
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
        else
        {
            Debug.LogError("Player camera not found!");
        }
    }
    
    // Show interaction text when player looks at the gun
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show UI prompt
            Debug.Log(interactText);
        }
    }
}