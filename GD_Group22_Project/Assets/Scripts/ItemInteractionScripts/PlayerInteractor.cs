using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float sphereCastRadius = 0.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Use SphereCast for wider detection
        if (Physics.SphereCast(ray, sphereCastRadius, out hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }

        // Debug visualization
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red, 1f);
    }

    // Optional: Visualize the sphere cast in the scene view
    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.blue;
            Vector3 origin = playerCamera.transform.position;
            Vector3 direction = playerCamera.transform.forward;
            Gizmos.DrawWireSphere(origin + direction * interactRange, sphereCastRadius);
            Gizmos.DrawLine(origin, origin + direction * interactRange);
        }
    }
}