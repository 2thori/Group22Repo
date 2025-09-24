using System.Collections.Generic;
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

    [Header("Glow Settings")]
    [SerializeField] private Color glowColor = Color.cyan;
    [SerializeField] private float glowIntensity = 2f;

    [Header("Gun Pickup Settings")]
    [SerializeField] private float gunPickupRadius = 2f;
    [SerializeField] private KeyCode pickUpGunKey = KeyCode.E;
    [SerializeField] private string gunPickupTag = "GravityGunPickup";

    // Private variables
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private bool isHoldingObject = false;
    private bool isGunEquipped = false;
    private bool isGunInInventory = false;
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

    // Beam/firing state
    private bool isFiring = false;

    private void Start()
    {
        InitializeComponents();
        SetupInputActions();
        SetupVisuals();

        // Start with gun unequipped and not in inventory
        UnequipGun();
        isGunInInventory = false;

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
            // We keep canceled if you later switch to a hold-while-fired scheme
            grabAction.action.canceled += ctx => { /* optionally handle hold-release */ };
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
            aimRayRenderer.enabled = false; // NOTE: beam is only enabled when fired
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

        // Only update visual beam positions when beam is visible (fired) or when holding
        if (aimRayRenderer != null && (aimRayRenderer.enabled || isHoldingObject))
        {
            UpdateVisualFeedback();
        }

        if (isHoldingObject)
        {
            HandleObjectMovement();
        }
    }

    private void CheckForGunPickup()
    {
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

        if (isGunInRange && Input.GetKeyDown(pickUpGunKey))
        {
            PickUpGun();
        }
    }

    private void PickUpGun()
    {
        if (gravityGunPickup != null)
        {
            Destroy(gravityGunPickup);
            gravityGunPickup = null;
            isGunInRange = false;

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

        // Do NOT enable the beam here — beam shows only when fired
        if (gunModel != null)
            gunModel.SetActive(true);

        Debug.Log("Gravity Gun Equipped!");
    }

    private void UnequipGun()
    {
        if (isHoldingObject)
            ReleaseObject();

        isGunEquipped = false;

        // Ensure beam off
        SetBeamActive(false);

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

        // If holding an object, lock the line to it
        if (isHoldingObject && heldObject != null)
        {
            rayEnd = heldObject.transform.position;
        }

        aimRayRenderer.SetPosition(0, rayStart);
        aimRayRenderer.SetPosition(1, rayEnd);

        bool canPickup = isHit && hitInfo.rigidbody != null && !hitInfo.rigidbody.isKinematic;
        if (aimRayRenderer.material != null)
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

        // Use standard Rigidbody property
        heldObjectRb.linearVelocity = (smoothPosition - heldObjectRb.position) / Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(playerCamera.transform.position - heldObjectRb.position);
        heldObjectRb.rotation = Quaternion.Slerp(heldObjectRb.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void OnGrab(InputAction.CallbackContext context)
    {
        if (!isGunEquipped) return;

        if (context.performed)
        {
            // Fire the beam (visible while trying to pick up and while holding)
            SetBeamActive(true);

            if (!isHoldingObject)
            {
                TryPickupObject();

                // If pickup failed, hide the beam
                if (!isHoldingObject)
                    SetBeamActive(false);
            }
            else
            {
                // If already holding, pressing grab releases the object
                ReleaseObject();
                SetBeamActive(false);
            }
        }
    }

    private void OnThrow(InputAction.CallbackContext context)
    {
        if (context.performed && isGunEquipped && isHoldingObject)
        {
            ThrowObject();
            SetBeamActive(false);
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

            // Apply glow (non-destructive)
            ApplyGlow(heldObject, true);

            Debug.Log($"Grabbed: {heldObject.name}");
        }
    }

    private void ReleaseObject()
    {
        if (heldObjectRb != null)
        {
            // Restore physics properties
            heldObjectRb.useGravity = originalGravity;
            heldObjectRb.linearDamping = originalDrag;
            heldObjectRb.constraints = originalConstraints;
        }

        // Remove glow before clearing references
        if (heldObject != null)
        {
            ApplyGlow(heldObject, false);
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
            // Store references to apply force after release
            Rigidbody rbToThrow = heldObjectRb;
            GameObject objToThrow = heldObject;

            ReleaseObject();

            // Apply impulse to stored object
            if (rbToThrow != null)
            {
                rbToThrow.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
                Debug.Log("Object thrown");
            }
        }
    }

    private void SetBeamActive(bool active)
    {
        if (aimRayRenderer == null) return;

        aimRayRenderer.enabled = active;
    }

    // Glow via MaterialPropertyBlock (non-destructive)
    private void ApplyGlow(GameObject obj, bool enable)
    {
        if (obj == null) return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r == null) continue;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);

            if (enable)
            {
                // Set emission color (shader must support _EmissionColor)
                block.SetColor("_EmissionColor", glowColor * glowIntensity);
                // To ensure emission is visible, enable keyword on the material instance if necessary
                // (MaterialPropertyBlock can't toggle keywords) - many standard shaders respect _EmissionColor only
            }
            else
            {
                block.SetColor("_EmissionColor", Color.black);
            }

            r.SetPropertyBlock(block);
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
        if (!isGunInInventory)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, gunPickupRadius);
        }

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
    public bool IsGunEquipped() => isGunEquipped;
    public bool IsGunInInventory() => isGunInInventory;
}
