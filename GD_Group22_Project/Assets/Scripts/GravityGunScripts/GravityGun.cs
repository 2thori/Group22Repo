using UnityEngine;

public class GravityGun : MonoBehaviour
{
    [Header("Gravity Gun Settings")]
    [SerializeField] private float pickupRange = 10f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float holdDistance = 3f;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode throwKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode rotateKey = KeyCode.Mouse1;

    private Camera playerCamera;
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private bool isHoldingObject = false;
    private float rotationSpeed = 5f;

    private void Start()
    {
        playerCamera = Camera.main;
        
        // Create hold position if not assigned
        if (holdPosition == null)
        {
            GameObject holdPosObj = new GameObject("HoldPosition");
            holdPosObj.transform.SetParent(playerCamera.transform);
            holdPosObj.transform.localPosition = new Vector3(0, 0, holdDistance);
            holdPosition = holdPosObj.transform;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (isHoldingObject)
            {
                ReleaseObject();
            }
            else
            {
                TryPickupObject();
            }
        }

        if (isHoldingObject)
        {
            // Move the object to the hold position
            if (heldObjectRb)
            {
                heldObjectRb.linearVelocity = Vector3.zero;
                heldObject.transform.position = Vector3.Lerp(
                    heldObject.transform.position, 
                    holdPosition.position, 
                    Time.deltaTime * 10f
                );

                // Rotate object if right mouse button is held
                if (Input.GetKey(rotateKey))
                {
                    float xRot = Input.GetAxis("Mouse X") * rotationSpeed;
                    float yRot = Input.GetAxis("Mouse Y") * rotationSpeed;
                    heldObject.transform.Rotate(playerCamera.transform.up, -xRot, Space.World);
                    heldObject.transform.Rotate(playerCamera.transform.right, yRot, Space.World);
                }

                // Throw object if left mouse button is pressed
                if (Input.GetKeyDown(throwKey))
                {
                    ThrowObject();
                }
            }
        }
    }

    private void TryPickupObject()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
    
        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            if (hit.collider.attachedRigidbody != null)
            {
                heldObject = hit.collider.gameObject;
                heldObjectRb = hit.collider.attachedRigidbody;
                heldObjectRb.useGravity = false;
                heldObjectRb.linearDamping = 10;
                heldObjectRb.constraints = RigidbodyConstraints.FreezeRotation;
                
                isHoldingObject = true;
            }
        }
    }

    private void ReleaseObject()
    {
        if (heldObjectRb)
        {
            heldObjectRb.useGravity = true;
            heldObjectRb.linearDamping = 1;
            heldObjectRb.constraints = RigidbodyConstraints.None;
        }
        
        heldObject = null;
        heldObjectRb = null;
        isHoldingObject = false;
    }

    private void ThrowObject()
    {
        if (heldObjectRb)
        {
            ReleaseObject();
            heldObjectRb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
        }
    }

    // Visualize the pickup range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}