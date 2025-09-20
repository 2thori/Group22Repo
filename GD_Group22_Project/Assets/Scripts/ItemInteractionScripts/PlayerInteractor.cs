using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Player Setup")]
    [Tooltip("The player's camera used for raycasting")]
    [SerializeField] private Camera playerCamera;

    [Tooltip("How far the player can reach to interact")]
    [SerializeField] private float interactRange = 3f;

    [Tooltip("Input Action for interaction")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Crosshair UI (optional)")]
    [Tooltip("If using an Image crosshair, assign it here")]
    [SerializeField] private Image crosshairImage;

    [Tooltip("If using TextMeshPro crosshair, assign it here")]
    [SerializeField] private TMP_Text crosshairText;

    [Tooltip("Default color for crosshair")]
    [SerializeField] private Color defaultColor = Color.white;

    [Tooltip("Color when looking at an interactable object")]
    [SerializeField] private Color highlightColor = Color.green;

    private IInteractable currentInteractable;

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteract;
    }

    private void Update()
    {
        UpdateCrosshair();
    }

    private void UpdateCrosshair()
    {
        currentInteractable = null;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
            }
        }

        // Update crosshair visuals
        if (crosshairImage != null)
            crosshairImage.color = currentInteractable != null ? highlightColor : defaultColor;

        if (crosshairText != null)
            crosshairText.color = currentInteractable != null ? highlightColor : defaultColor;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        // Ignore if pointing at UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}
