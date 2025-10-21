using UnityEngine;
using TMPro;

public class NT_PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Camera PlayerCamera;
    public float InteractionDistance = 3f;
    public LayerMask interactionLayerMask = -1;

    [Header("UI References")]
    public GameObject interactionText;
    
    private NT_interactObject currentInteractable;
    private DoorInteractable currentDoor;
    private ChemicalWorkbench currentWorkbench;
    private GravityGunWorkbench currentGunWorkbench;
    private WinObject currentWinObject;
    private TextMeshProUGUI textComponent;

    private void Start()
    {
        if (interactionText != null)
        {
            textComponent = interactionText.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        HandleInteractionRaycast();
        HandleInteractionInput();
    }

    private void HandleInteractionRaycast()
    {
        Ray ray = PlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, InteractionDistance, interactionLayerMask))
        {
            HandleInteractableHit(hit);
        }
        else
        {
            ClearCurrentInteractables();
        }
    }

    private void HandleInteractableHit(RaycastHit hit)
    {
        // Check for WinObject first (highest priority)
        WinObject winObject = hit.collider.GetComponent<WinObject>();
        if (winObject != null && winObject.CanInteract())
        {
            SetCurrentInteractable(winObject, winObject.GetInteractionText());
            return;
        }

        // Check for other interactables
        NT_interactObject interactObject = hit.collider.GetComponent<NT_interactObject>();
        DoorInteractable doorObject = hit.collider.GetComponent<DoorInteractable>();
        ChemicalWorkbench workbenchObject = hit.collider.GetComponent<ChemicalWorkbench>();
        GravityGunWorkbench gunWorkbenchObject = hit.collider.GetComponent<GravityGunWorkbench>();

        if (interactObject != null)
        {
            SetCurrentInteractable(interactObject, interactObject.GetInteractionText());
            return;
        }

        if (doorObject != null)
        {
            SetCurrentInteractable(doorObject, doorObject.GetInteractionText());
            return;
        }

        if (workbenchObject != null)
        {
            SetCurrentInteractable(workbenchObject, workbenchObject.GetInteractionText());
            return;
        }

        if (gunWorkbenchObject != null)
        {
            SetCurrentInteractable(gunWorkbenchObject, gunWorkbenchObject.GetInteractionText());
            return;
        }

        ClearCurrentInteractables();
    }

    private void SetCurrentInteractable(object interactable, string interactionText)
    {
        if (interactable != currentInteractable && 
            interactable != currentDoor && 
            interactable != currentWorkbench && 
            interactable != currentGunWorkbench && 
            interactable != currentWinObject)
        {
            ClearCurrentInteractables();
        }

        if (interactable is WinObject winObj)
        {
            currentWinObject = winObj;
            ShowInteractionText(interactionText);
        }
        else if (interactable is NT_interactObject interactObj)
        {
            currentInteractable = interactObj;
            ShowInteractionText(interactionText);
        }
        else if (interactable is DoorInteractable doorObj)
        {
            currentDoor = doorObj;
            ShowInteractionText(interactionText);
        }
        else if (interactable is ChemicalWorkbench workbenchObj)
        {
            currentWorkbench = workbenchObj;
            ShowInteractionText(interactionText);
        }
        else if (interactable is GravityGunWorkbench gunWorkbenchObj)
        {
            currentGunWorkbench = gunWorkbenchObj;
            ShowInteractionText(interactionText);
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentWinObject != null && currentWinObject.CanInteract())
            {
                currentWinObject.Interact();
                ClearCurrentInteractables();
            }
            else if (currentInteractable != null)
            {
                currentInteractable.Interact();
                UpdateInteractionText();
            }
            else if (currentDoor != null)
            {
                currentDoor.ToggleDoor();
                UpdateInteractionText();
            }
            else if (currentWorkbench != null)
            {
                currentWorkbench.Interact();
                UpdateInteractionText();
            }
            else if (currentGunWorkbench != null)
            {
                currentGunWorkbench.Interact();
                UpdateInteractionText();
            }
        }
    }

    private void UpdateInteractionText()
    {
        if (!interactionText.activeInHierarchy) return;

        string newText = GetCurrentInteractionText();
        if (!string.IsNullOrEmpty(newText))
        {
            ShowInteractionText(newText);
        }
        else
        {
            HideInteractionText();
        }
    }

    private string GetCurrentInteractionText()
    {
        if (currentWinObject != null && currentWinObject.CanInteract())
            return currentWinObject.GetInteractionText();
        if (currentInteractable != null)
            return currentInteractable.GetInteractionText();
        if (currentDoor != null)
            return currentDoor.GetInteractionText();
        if (currentWorkbench != null)
            return currentWorkbench.GetInteractionText();
        if (currentGunWorkbench != null)
            return currentGunWorkbench.GetInteractionText();
        
        return string.Empty;
    }

    private void ShowInteractionText(string text)
    {
        if (interactionText != null && textComponent != null && !string.IsNullOrEmpty(text))
        {
            textComponent.text = text;
            interactionText.SetActive(true);
        }
    }

    private void HideInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

    private void ClearCurrentInteractables()
    {
        bool hadInteractable = currentInteractable != null || currentDoor != null || 
                              currentWorkbench != null || currentGunWorkbench != null || 
                              currentWinObject != null;

        currentInteractable = null;
        currentDoor = null;
        currentWorkbench = null;
        currentGunWorkbench = null;
        currentWinObject = null;

        if (hadInteractable)
        {
            HideInteractionText();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (PlayerCamera != null)
        {
            Gizmos.color = Color.green;
            Vector3 rayOrigin = PlayerCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0.1f));
            Vector3 rayDirection = PlayerCamera.transform.forward * InteractionDistance;
            Gizmos.DrawRay(rayOrigin, rayDirection);
        }
    }
}