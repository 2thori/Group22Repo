using UnityEngine;

public class GravityGun : MonoBehaviour
{
    [SerializeField] private GameObject gravityGunModel;
    [SerializeField] private GameObject pickupPrompt;
    
    private bool canBePickedUp = false;
    private bool hasBeenPickedUp = false;

    private void Start()
    {
        // Initially disable the gravity gun
        if (gravityGunModel != null)
        {
            gravityGunModel.SetActive(false);
        }
        
        // Hide pickup prompt initially
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
        
        // Subscribe to the event for when all parts are collected
        GravityGunPartsInventory.OnAllPartsCollected += EnableGravityGun;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event
        GravityGunPartsInventory.OnAllPartsCollected -= EnableGravityGun;
    }

    private void EnableGravityGun()
    {
        canBePickedUp = true;
        
        // Enable the gravity gun model
        if (gravityGunModel != null)
        {
            gravityGunModel.SetActive(true);
        }
        
        Debug.Log("Gravity gun is now available for pickup!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canBePickedUp && !hasBeenPickedUp)
        {
            // Show pickup prompt
            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && canBePickedUp && !hasBeenPickedUp)
        {
            // Check for input to pick up the gravity gun
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickupGravityGun(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }

    private void PickupGravityGun(GameObject player)
    {
        hasBeenPickedUp = true;
        
        // Hide the gravity gun model and prompt
        if (gravityGunModel != null)
        {
            gravityGunModel.SetActive(false);
        }
        
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
        
        // Enable gravity gun functionality on the player
        // You would add your gravity gun script to the player here
        // For example: player.AddComponent<GravityGunController>();
        
        Debug.Log("Gravity gun picked up!");
        
        // Notify other systems that the gravity gun was picked up
        // You could add an event here similar to the parts collection event
    }
    
    // For debugging purposes
    private void Update()
    {
        // Debug command to enable the gravity gun without collecting all parts
        if (Input.GetKeyDown(KeyCode.P) && !canBePickedUp)
        {
            EnableGravityGun();
            Debug.Log("DEBUG: Gravity gun enabled via cheat code");
        }
    }
}