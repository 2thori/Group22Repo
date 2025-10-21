using UnityEngine;
using System.Collections;

public class RobustAreaTrigger : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private string areaName;
    [SerializeField] private bool showOnlyOnce = true;
    [SerializeField] private float cooldownTime = 2f; // Prevent rapid re-triggering

    [Header("Visual Feedback")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(1, 0.5f, 0, 0.3f);

    private Collider triggerCollider;
    private bool isOnCooldown = false;
    private bool hasBeenTriggeredThisSession = false;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        
        if (triggerCollider == null)
        {
            Debug.LogError("RobustAreaTrigger requires a Collider component!");
            return;
        }

        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOnCooldown)
        {
            StartCoroutine(TriggerCooldown());
            
            if (ShouldShowPopup())
            {
                ShowPopup();
            }
        }
    }

    private IEnumerator TriggerCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    private bool ShouldShowPopup()
    {
        // If showOnlyOnce is false, always show (after cooldown)
        if (!showOnlyOnce) return true;

        // Check global popup history
        if (PopupManager.Instance != null && PopupManager.Instance.HasPopupBeenShown(areaName))
            return false;

        // Check session history
        if (hasBeenTriggeredThisSession)
            return false;

        return true;
    }

    private void ShowPopup()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowAreaPopup(areaName);
            hasBeenTriggeredThisSession = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        
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
        }

        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, $"{areaName}\n(Once: {showOnlyOnce})");
        #endif
    }
}