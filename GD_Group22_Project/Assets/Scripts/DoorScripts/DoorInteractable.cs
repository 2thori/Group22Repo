using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    public enum DoorType { Rotating, Sliding }

    [Header("Door Type")]
    [SerializeField] private DoorType doorType = DoorType.Rotating;

    [Header("Rotation Settings")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closeAngle = 0f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Sliding Settings")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private Vector3 slideDirection = Vector3.right;
    [SerializeField] private float slideSpeed = 2f;

    [Header("Lock Settings")]
    [SerializeField] private bool islocked = false;
    [SerializeField] private Key requiredKey;

    private bool isDoorOpen = false;
    private bool isAnimating = false;
    
    // Rotation variables
    private Quaternion targetRotation;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    // Sliding variables
    private Vector3 targetPosition;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    private void Start()
    {
        InitializeDoor();
    }

    private void InitializeDoor()
    {
        if (doorType == DoorType.Rotating)
        {
            // Initialize rotation settings
            closedRotation = Quaternion.Euler(doorPivot.localEulerAngles.x, closeAngle, doorPivot.localEulerAngles.z);
            openRotation = Quaternion.Euler(doorPivot.localEulerAngles.x, openAngle, doorPivot.localEulerAngles.z);
            targetRotation = closedRotation;
            doorPivot.localRotation = closedRotation;
        }
        else // Sliding
        {
            // Initialize sliding settings
            closedPosition = doorTransform.localPosition;
            openPosition = closedPosition + slideDirection.normalized * slideDistance;
            targetPosition = closedPosition;
            doorTransform.localPosition = closedPosition;
        }
    }

    private void Update()
    {
        if (isAnimating)
        {
            if (doorType == DoorType.Rotating)
            {
                // Rotate smoothly towards the target rotation
                doorPivot.localRotation = Quaternion.RotateTowards(
                    doorPivot.localRotation,
                    targetRotation,
                    rotationSpeed * 100f * Time.deltaTime
                );

                // Stop animating when close enough
                if (Quaternion.Angle(doorPivot.localRotation, targetRotation) < 0.1f)
                {
                    doorPivot.localRotation = targetRotation;
                    isAnimating = false;
                }
            }
            else // Sliding
            {
                // Move smoothly towards the target position
                doorTransform.localPosition = Vector3.MoveTowards(
                    doorTransform.localPosition,
                    targetPosition,
                    slideSpeed * Time.deltaTime
                );

                // Stop animating when close enough to target position
                if (Vector3.Distance(doorTransform.localPosition, targetPosition) < 0.01f)
                {
                    doorTransform.localPosition = targetPosition;
                    isAnimating = false;
                }
            }
        }
    }

    public void ToggleDoor()
    {
        if (islocked)
        {
            if (KeyInventory.Instance.Haskey(requiredKey))
            {
                Debug.Log($"Door '{gameObject.name}' unlocked with the correct key: {requiredKey.keyName}.");
                islocked = false;
            }
            else
            {
                Debug.Log($"Door '{gameObject.name}' is locked. You need the correct key: {requiredKey.keyName}.");
                return;
            }
        }

        if (isAnimating) return;

        isDoorOpen = !isDoorOpen;

        if (doorType == DoorType.Rotating)
        {
            targetRotation = isDoorOpen ? openRotation : closedRotation;
        }
        else // Sliding
        {
            targetPosition = isDoorOpen ? openPosition : closedPosition;
        }

        isAnimating = true;
    }

    // Method to change door type at runtime if needed
    public void SetDoorType(DoorType newType)
    {
        if (doorType != newType)
        {
            // Reset to closed state before switching
            if (isAnimating) isAnimating = false;
            isDoorOpen = false;
            
            doorType = newType;
            InitializeDoor();
        }
    }

    // Optional: Gizmos to visualize both types in the editor
    private void OnDrawGizmosSelected()
    {
        if (doorType == DoorType.Rotating && doorPivot != null)
        {
            // Draw rotation arc
            Gizmos.color = Color.blue;
            Gizmos.matrix = doorPivot.localToWorldMatrix;
            DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, openAngle, 1f);
        }
        else if (doorType == DoorType.Sliding && doorTransform != null)
        {
            // Draw slide direction
            Gizmos.color = Color.green;
            Vector3 worldClosedPos = transform.TransformPoint(closedPosition);
            Vector3 worldOpenPos = transform.TransformPoint(closedPosition + slideDirection.normalized * slideDistance);
            
            Gizmos.DrawLine(worldClosedPos, worldOpenPos);
            Gizmos.DrawWireCube(worldOpenPos, Vector3.one * 0.1f);
            Gizmos.DrawWireCube(worldClosedPos, Vector3.one * 0.1f);
        }
    }

    // Helper method for drawing rotation arc
    private void DrawWireArc(Vector3 position, Vector3 axis, Vector3 from, float angle, float radius)
    {
        int segments = 20;
        float step = angle / segments;
        Quaternion rotation = Quaternion.AngleAxis(step, axis);
        
        Vector3 current = from * radius;
        for (int i = 0; i < segments; i++)
        {
            Vector3 next = rotation * current;
            Gizmos.DrawLine(position + current, position + next);
            current = next;
        }
    }
}