using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closeAngle = 0f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Lock Settings")]
    [SerializeField] private bool islocked = false;

    [SerializeField] private Key requiredKey;
    

    private bool isDoorOpen = false;
    private bool isAnimating = false;
    private Quaternion targetRotation;

    private void Start()
    {
        // Initialize to closed rotation
        targetRotation = Quaternion.Euler(doorPivot.localEulerAngles.x, closeAngle, doorPivot.localEulerAngles.z);
        doorPivot.localRotation = targetRotation;
    }

    private void Update()
    {
        if (isAnimating)
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

        // Set the target rotation, keeping X and Z the same
        targetRotation = Quaternion.Euler(
            doorPivot.localEulerAngles.x,
            isDoorOpen ? openAngle : closeAngle,
            doorPivot.localEulerAngles.z
        );

        isAnimating = true;
    }
}