using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private string areaName;
    [SerializeField] private bool showOnlyOnce = true;

    [Header("Visual Feedback")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(1, 0.5f, 0, 0.3f); // Orange
    [SerializeField] private bool showDebugLogs = false;

    private Collider triggerCollider;
    private bool hasBeenTriggeredThisSession = false;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        
        if (triggerCollider == null)
        {
            Debug.LogError("AreaTrigger requires a Collider component!");
            return;
        }

        triggerCollider.isTrigger = true;

        if (showDebugLogs)
        {
            bool alreadyShown = PopupManager.Instance != null && PopupManager.Instance.HasPopupBeenShown(areaName);
            Debug.Log($"AreaTrigger '{areaName}' started. Already shown: {alreadyShown}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugLogs)
                Debug.Log($"Player entered trigger: {areaName}");

            // Check if we should show the popup
            if (ShouldShowPopup())
            {
                ShowPopup();
            }
        }
    }

    private bool ShouldShowPopup()
    {
        // If showOnlyOnce is false, always show
        if (!showOnlyOnce) return true;

        // Check if popup manager says this has been shown before
        if (PopupManager.Instance != null && PopupManager.Instance.HasPopupBeenShown(areaName))
        {
            if (showDebugLogs)
                Debug.Log($"Popup for {areaName} has already been shown globally. Skipping.");
            return false;
        }

        // Additional session-based check (optional safety)
        if (hasBeenTriggeredThisSession)
        {
            if (showDebugLogs)
                Debug.Log($"Popup for {areaName} has been triggered this session. Skipping.");
            return false;
        }

        return true;
    }

    private void ShowPopup()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowAreaPopup(areaName);
            hasBeenTriggeredThisSession = true;
            
            if (showDebugLogs)
                Debug.Log($"Showing popup for area: {areaName}");
        }
        else
        {
            Debug.LogWarning("PopupManager instance not found!");
        }
    }

    // Visualize the trigger area in the editor
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        
        // Draw the trigger area
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else if (col is SphereCollider sphereCollider)
            {
                Gizmos.DrawSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
            else if (col is CapsuleCollider capsuleCollider)
            {
                // Simplified capsule visualization
                Gizmos.DrawWireSphere(transform.position + capsuleCollider.center, capsuleCollider.radius);
            }
        }

        // Draw label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, $"{areaName}\n(Once: {showOnlyOnce})");
        #endif
    }

    // Reset this trigger for current session (for testing)
    public void ResetTriggerForSession()
    {
        hasBeenTriggeredThisSession = false;
        if (showDebugLogs)
            Debug.Log($"Reset session trigger for: {areaName}");
    }

    // Force show the popup (for debugging)
    public void ForceShowPopup()
    {
        ShowPopup();
    }
}