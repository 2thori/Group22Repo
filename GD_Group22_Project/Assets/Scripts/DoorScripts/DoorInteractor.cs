using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DoorInteractor : MonoBehaviour
{
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private InputActionReference interactionAction; // Assign E key action
    [SerializeField] private Image crosshair;

    private Camera _camera;
    private DoorItem doorItem;

    private void Start()
    {
        _camera = Camera.main;
        interactionAction.action.Enable(); // enable the input action
    }

    private void Update()
    {
        PerformRaycast();
    }

    private void PerformRaycast()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            DoorItem hitItem = hit.collider.GetComponent<DoorItem>();
            if (hitItem != null)
            {
                doorItem = hitItem;
                HighlightCrosshair(true);

                // Check input
                if (interactionAction.action.WasPressedThisFrame())
                {
                    doorItem.ObjectInteraction();
                }
            }
            else
            {
                ClearItem();
            }
        }
        else
        {
            ClearItem();
        }
    }

    private void ClearItem()
    {
        if (doorItem != null)
        {
            doorItem = null;
            HighlightCrosshair(false);
        }
    }

    private void HighlightCrosshair(bool on)
    {
        if (crosshair != null)
            crosshair.color = on ? Color.red : Color.white;
    }
}