using UnityEngine;

using UnityEngine;

public class KeyCollectable : MonoBehaviour
{
    [SerializeField] private Key key;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;
   
    private AudioSource audioSource;
   
    private void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.playOnAwake = false;
        }
    }
   
    public void KeyPickup()
    {
        if (key != null)
        {
            KeyInventory.Instance.AddKey(key);
         
            // Play pickup sound if assigned
            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound, soundVolume);
            }
         
            // Wait for sound to finish playing before deactivating (optional)
            if (pickupSound != null && audioSource != null)
            {
                // Deactivate after sound finishes
                Invoke("DeactivateObject", pickupSound.length);
            }
            else
            {
                DeactivateObject();
            }
        }
    }
   
    private void DeactivateObject()
    {
        gameObject.SetActive(false);
    }
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            KeyPickup();
        }
    }
}