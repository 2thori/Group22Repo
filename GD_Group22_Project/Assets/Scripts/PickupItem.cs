using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Picked up: " + gameObject.name);
        Destroy(gameObject); // removes the object
    }
}