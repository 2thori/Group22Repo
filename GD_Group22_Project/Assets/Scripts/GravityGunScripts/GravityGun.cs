using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class GravityGun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private GameObject gunModel;
    [SerializeField] private LineRenderer aimRayRenderer;

    [Header("Gravity Gun Settings")]
    [SerializeField] private float pickupRange = 10f;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float holdDistance = 3f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private LayerMask pickupLayer = ~0;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference equipAction;
    [SerializeField] private InputActionReference grabAction;
    [SerializeField] private InputActionReference throwAction;

    [Header("Visual Settings")]
    [SerializeField] private Material canPickupMaterial;
    [SerializeField] private Material cannotPickupMaterial;
    [SerializeField] private float lineWidth = 0.02f;

    [Header("Gun Pickup Settings")]
    [SerializeField] private float gunPickupRadius = 2f;
    [SerializeField] private KeyCode pickUpGunKey = KeyCode.E;
    [SerializeField] private string gunPickupTag = "GravityGunPickup";

    // Private variables
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private bool isHoldingObject = false;
    private bool isGunEquipped = false;
    private bool isGunInInventory = false; // Track if player has picked up the gun
    private Vector3 currentHoldOffset;
    private RaycastHit hitInfo;
    private bool isHit;

    // Original object properties
    private float originalDrag;
    private bool originalGravity;
    private RigidbodyConstraints originalConstraints;

    // Gun pickup variables
    private GameObject gravityGunPickup;
    private bool isGunInRange = false;

    private void Start()
    {
        InitializeComponents();
        SetupInputActions();
        SetupVisuals();
        
        // Start with gun unequipped and not in inventory
        UnequipGun();
        isGunInInventory = false;
        
        // Make sure gun model is hidden initially
        if (gunModel != null)
            gunModel.SetActive(false);
    }

    private void InitializeComponents()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            Debug.LogError("No camera found! Assign a camera to the Gravity Gun.");

        if (holdPosition == null)
        {
            GameObject holdPosObj = new GameObject("HoldPosition");
            holdPosObj.transform.SetParent(playerCamera.transform);
            holdPosObj.transform.localPosition = new Vector3(0, 0, holdDistance);
            holdPosition = holdPosObj.transform;
        }

        if (aimRayRenderer == null)
            aimRayRenderer = GetComponent<LineRenderer>();

        currentHoldOffset = new Vector3(0, 0, holdDistance);
    }

    private void SetupInputActions()
    {
        if (equipAction != null)
        {
            equipAction.action.Enable();
            equipAction.action.performed += OnEquipToggle;
        }

        if (grabAction != null)
        {
            grabAction.action.Enable();
            grabAction.action.performed += OnGrab;
        }

        if (throwAction != null)
        {
            throwAction.action.Enable();
            throwAction.action.performed += OnThrow;
        }
    }

    private void SetupVisuals()
    {
        if (aimRayRenderer != null)
        {
            aimRayRenderer.positionCount = 2;
            aimRayRenderer.startWidth = lineWidth;
            aimRayRenderer.endWidth = lineWidth;
            aimRayRenderer.enabled = false;
        }
    }

    private void Update()
    {
        // Check for gun pickup if we don't have it in inventory yet
        if (!isGunInInventory)
        {
            CheckForGunPickup();
        }
        
        // Only process gravity gun functionality if equipped
        if (!isGunEquipped) return;

        UpdateRaycast();
        UpdateVisualFeedback();

        if (isHoldingObject)
        {
            HandleObjectMovement();
        }
    }

    private void CheckForGunPickup()
    {
        // Look for gravity gun pickup in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, gunPickupRadius);
        isGunInRange = false;
        gravityGunPickup = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(gunPickupTag))
            {
                isGunInRange = true;
                gravityGunPickup = hitCollider.gameObject;
                break;
            }
        }

        // Check for pickup input
        if (isGunInRange && Input.GetKeyDown(pickUpGunKey))
        {
            PickUpGun();
        }
    }

    private void PickUpGun()
    {
        if (gravityGunPickup != null)
        {
            // Destroy the pickup object
            Destroy(gravityGunPickup);
            gravityGunPickup = null;
            isGunInRange = false;
            
            // Add gun to inventory and auto-equip it
            isGunInInventory = true;
            EquipGun();
            
            Debug.Log("Gravity Gun added to inventory!");
        }
    }

    private void OnEquipToggle(InputAction.CallbackContext context)
    {
        if (!context.performed || !isGunInInventory) return;
        
        if (isGunEquipped)
        {
            UnequipGun();
        }
        else
        {
            EquipGun();
        }
    }

    private void EquipGun()
    {
        if (!isGunInInventory) return;
        
        isGunEquipped = true;
        
        // Enable visuals
        if (aimRayRenderer != null)
            aimRayRenderer.enabled = true;
        
        if (gunModel != null)
            gunModel.SetActive(true);

        Debug.Log("Gravity Gun Equipped!");
    }

    private void UnequipGun()
    {
        // Release any held object
        if (isHoldingObject)
            ReleaseObject();
        
        isGunEquipped = false;
        
        // Disable visuals
        if (aimRayRenderer != null)
            aimRayRenderer.enabled = false;
        
        if (gunModel != null)
            gunModel.SetActive(false);

        Debug.Log("Gravity Gun Unequipped!");
    }

    private void UpdateRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        isHit = Physics.Raycast(ray, out hitInfo, pickupRange, pickupLayer);
    }

    private void UpdateVisualFeedback()
    {
        if (aimRayRenderer == null || playerCamera == null) return;

        Vector3 rayStart = playerCamera.transform.position;
        Vector3 rayEnd = isHit ? hitInfo.point : rayStart + playerCamera.transform.forward * pickupRange;

        aimRayRenderer.SetPosition(0, rayStart);
        aimRayRenderer.SetPosition(1, rayEnd);

        bool canPickup = isHit && hitInfo.rigidbody != null && !hitInfo.rigidbody.isKinematic;
        aimRayRenderer.material = canPickup ? canPickupMaterial : cannotPickupMaterial;
    }

    private void HandleObjectMovement()
    {
        if (!isHoldingObject || heldObjectRb == null || playerCamera == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        if (mouseDelta.magnitude > 0.1f)
        {
            Vector3 movement = new Vector3(
                mouseDelta.x * mouseSensitivity * Time.deltaTime,
                mouseDelta.y * mouseSensitivity * Time.deltaTime,
                0
            );

            Vector3 cameraRight = playerCamera.transform.right;
            Vector3 cameraUp = playerCamera.transform.up;
            
            currentHoldOffset += cameraRight * movement.x;
            currentHoldOffset += cameraUp * movement.y;
            
            float scroll = Mouse.current.scroll.ReadValue().y * 0.1f;
            currentHoldOffset.z = Mathf.Clamp(currentHoldOffset.z + scroll, 1f, pickupRange);
        }

        holdPosition.localPosition = Vector3.Lerp(holdPosition.localPosition, currentHoldOffset, Time.deltaTime * 5f);

        Vector3 targetPosition = holdPosition.position;
        Vector3 smoothPosition = Vector3.Lerp(heldObjectRb.position, targetPosition, smoothSpeed * Time.deltaTime);
        
        heldObjectRb.linearVelocity = (smoothPosition - heldObjectRb.position) / Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(playerCamera.transform.position - heldObjectRb.position);
        heldObjectRb.rotation = Quaternion.Slerp(heldObjectRb.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void OnGrab(InputAction.CallbackContext context)
    {
        if (!isGunEquipped) return;

        if (context.performed)
        {
            if (!isHoldingObject)
            {
                TryPickupObject();
            }
            else
            {
                ReleaseObject();
            }
        }
    }

    private void OnThrow(InputAction.CallbackContext context)
    {
        if (context.performed && isGunEquipped && isHoldingObject)
        {
            ThrowObject();
        }
    }

    private void TryPickupObject()
    {
        if (isHit && hitInfo.rigidbody != null && !hitInfo.rigidbody.isKinematic)
        {
            heldObject = hitInfo.collider.gameObject;
            heldObjectRb = hitInfo.rigidbody;

            // Save original properties
            originalDrag = heldObjectRb.linearDamping;
            originalGravity = heldObjectRb.useGravity;
            originalConstraints = heldObjectRb.constraints;

            // Modify properties for holding
            heldObjectRb.useGravity = false;
            heldObjectRb.linearDamping = 10f;
            heldObjectRb.constraints = RigidbodyConstraints.FreezeRotation;
            heldObjectRb.angularVelocity = Vector3.zero;

            // Reset hold position
            currentHoldOffset = new Vector3(0, 0, holdDistance);
            holdPosition.localPosition = currentHoldOffset;

            isHoldingObject = true;

            Debug.Log($"Grabbed: {heldObject.name}");
        }
    }

    private void ReleaseObject()
    {
        if (heldObjectRb != null)
        {
            // Restore original properties
            heldObjectRb.useGravity = originalGravity;
            heldObjectRb.linearDamping = originalDrag;
            heldObjectRb.constraints = originalConstraints;
        }

        heldObject = null;
        heldObjectRb = null;
        isHoldingObject = false;

        Debug.Log("Object released");
    }

    private void ThrowObject()
    {
        if (heldObjectRb != null)
        {
            ReleaseObject();
            heldObjectRb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
            Debug.Log("Object thrown");
        }
    }

    private void OnEnable()
    {
        if (equipAction != null) equipAction.action.Enable();
        if (grabAction != null) grabAction.action.Enable();
        if (throwAction != null) throwAction.action.Enable();
    }

    private void OnDisable()
    {
        if (isHoldingObject)
            ReleaseObject();
        
        if (isGunEquipped)
            UnequipGun();

        if (equipAction != null) equipAction.action.Disable();
        if (grabAction != null) grabAction.action.Disable();
        if (throwAction != null) throwAction.action.Disable();
    }

    private void OnDestroy()
    {
        if (equipAction != null)
            equipAction.action.performed -= OnEquipToggle;
        if (grabAction != null)
            grabAction.action.performed -= OnGrab;
        if (throwAction != null)
            throwAction.action.performed -= OnThrow;
    }

    // Visualize in editor
    private void OnDrawGizmosSelected()
    {
        // Draw gun pickup radius (only if we don't have the gun yet)
        if (!isGunInInventory)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, gunPickupRadius);
        }

        // Draw gravity gun range and hold position
        if (playerCamera != null && isGunEquipped)
        {
            Gizmos.color = Color.yellow;
            Vector3 rayStart = playerCamera.transform.position;
            Vector3 rayEnd = rayStart + playerCamera.transform.forward * pickupRange;
            Gizmos.DrawLine(rayStart, rayEnd);

            if (isHit)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hitInfo.point, 0.1f);
            }

            if (holdPosition != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(holdPosition.position, 0.2f);
            }
        }
    }

    // Public methods for other scripts to interact with the gravity gun
    public bool IsGunEquipped()
    {
        return isGunEquipped;
    }

    public bool IsGunInInventory()
    {
        return isGunInInventory;
    }

    public bool IsGunPickupInRange()
    {
        return isGunInRange;
    }

    // Method to force add the gun to inventory (for debugging or special cases)
    public void AddGunToInventory()
    {
        isGunInInventory = true;
        EquipGun();
    }
}