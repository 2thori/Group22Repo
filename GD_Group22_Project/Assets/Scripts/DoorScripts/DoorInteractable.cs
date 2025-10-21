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
    [SerializeField] private bool isLocked = false;
    [SerializeField] private Key requiredKey;

    [Header("Interaction Text")]
    [SerializeField] private string lockedText = "Locked - Need Key";
    [SerializeField] private string openText = "Press E to Open";
    [SerializeField] private string closeText = "Press E to Close";

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

    // Public property to check if door is locked
    public bool IsLocked 
    { 
        get { return isLocked; } 
        set { isLocked = value; }
    }

    private void Start()
    {
        InitializeDoor();
    }

    private void InitializeDoor()
    {
        if (doorType == DoorType.Rotating)
        {
            if (doorPivot != null)
            {
                closedRotation = Quaternion.Euler(doorPivot.localEulerAngles.x, closeAngle, doorPivot.localEulerAngles.z);
                openRotation = Quaternion.Euler(doorPivot.localEulerAngles.x, openAngle, doorPivot.localEulerAngles.z);
                targetRotation = closedRotation;
                doorPivot.localRotation = closedRotation;
            }
        }
        else
        {
            if (doorTransform != null)
            {
                closedPosition = doorTransform.localPosition;
                openPosition = closedPosition + slideDirection.normalized * slideDistance;
                targetPosition = closedPosition;
                doorTransform.localPosition = closedPosition;
            }
        }
    }

    private void Update()
    {
        HandleDoorAnimation();
    }

    private void HandleDoorAnimation()
    {
        if (isAnimating)
        {
            if (doorType == DoorType.Rotating && doorPivot != null)
            {
                doorPivot.localRotation = Quaternion.RotateTowards(
                    doorPivot.localRotation,
                    targetRotation,
                    rotationSpeed * 100f * Time.deltaTime
                );

                if (Quaternion.Angle(doorPivot.localRotation, targetRotation) < 0.1f)
                {
                    doorPivot.localRotation = targetRotation;
                    isAnimating = false;
                }
            }
            else if (doorType == DoorType.Sliding && doorTransform != null)
            {
                doorTransform.localPosition = Vector3.MoveTowards(
                    doorTransform.localPosition,
                    targetPosition,
                    slideSpeed * Time.deltaTime
                );

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
        if (isLocked)
        {
            // Check if player has the required key
            bool hasKey = CheckForKey();
            
            if (hasKey)
            {
                Debug.Log($"Door '{gameObject.name}' unlocked with the correct key!");
                isLocked = false;
            }
            else
            {
                Debug.Log($"Door '{gameObject.name}' is locked. You need the correct key!");
                return;
            }
        }

        if (isAnimating) return;

        isDoorOpen = !isDoorOpen;

        if (doorType == DoorType.Rotating && doorPivot != null)
        {
            targetRotation = isDoorOpen ? openRotation : closedRotation;
        }
        else if (doorType == DoorType.Sliding && doorTransform != null)
        {
            targetPosition = isDoorOpen ? openPosition : closedPosition;
        }

        isAnimating = true;
    }

    private bool CheckForKey()
    {
        // Check if we have a KeyInventory system and if we have the required key
        if (requiredKey == null)
        {
            Debug.LogWarning("Door is locked but no required key is set!");
            return false;
        }

        if (KeyInventory.Instance != null)
        {
            return KeyInventory.Instance.Haskey(requiredKey);
        }
        
        Debug.LogWarning("No KeyInventory instance found!");
        return false;
    }

    // Method for the interaction system to get the appropriate text
    public string GetInteractionText()
    {
        if (isLocked)
        {
            if (requiredKey != null)
            {
                return $"{lockedText} ({requiredKey.keyName})";
            }
            return lockedText;
        }
        else
        {
            return isDoorOpen ? closeText : openText;
        }
    }

    // Method to lock/unlock the door
    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    // Method to unlock the door with a key (for external scripts)
    public void UnlockDoor(Key key)
    {
        if (key == requiredKey)
        {
            isLocked = false;
            Debug.Log($"Door '{gameObject.name}' unlocked!");
        }
    }

    // Optional: Gizmos to visualize both types in the editor
    private void OnDrawGizmosSelected()
    {
        if (doorType == DoorType.Rotating && doorPivot != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.matrix = doorPivot.localToWorldMatrix;
            DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, openAngle, 1f);
        }
        else if (doorType == DoorType.Sliding && doorTransform != null)
        {
            Gizmos.color = Color.green;
            Vector3 worldClosedPos = transform.TransformPoint(closedPosition);
            Vector3 worldOpenPos = transform.TransformPoint(closedPosition + slideDirection.normalized * slideDistance);
            
            Gizmos.DrawLine(worldClosedPos, worldOpenPos);
            Gizmos.DrawWireCube(worldOpenPos, Vector3.one * 0.1f);
            Gizmos.DrawWireCube(worldClosedPos, Vector3.one * 0.1f);
        }
    }

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