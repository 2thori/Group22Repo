using UnityEngine;

public class AxisArrows : MonoBehaviour
{
    [Header("Assign arrow GameObjects (optional - will auto-find children named XAxis/YAxis/ZAxis)")]
    [SerializeField] private GameObject XAxis;
    [SerializeField] private GameObject YAxis;
    [SerializeField] private GameObject ZAxis;

    private bool _arrowActive;
    private PhysicsGunInteractionBehavior _gun;

    private void Awake()
    {
        // Try auto-find children if not assigned
        if (XAxis == null) XAxis = transform.Find("XAxis")?.gameObject;
        if (YAxis == null) YAxis = transform.Find("YAxis")?.gameObject;
        if (ZAxis == null) ZAxis = transform.Find("ZAxis")?.gameObject;
    }

    private void OnEnable()
    {
        _gun = FindObjectOfType<PhysicsGunInteractionBehavior>();
        if (_gun != null)
        {
            _gun.OnRotation.AddListener(EnableArrows);
        }

        // Ensure arrows start hidden
        SetArrowsActive(false);
    }

    private void OnDisable()
    {
        if (_gun != null)
            _gun.OnRotation.RemoveListener(EnableArrows);
    }

    private void Start()
    {
        // defensive: also ensure they are inactive on start
        SetArrowsActive(false);
    }

    /// <summary>Called by gun's OnRotation UnityEvent</summary>
    public void EnableArrows(bool enable)
    {
        _arrowActive = enable;
        SetArrowsActive(enable);

        if (!enable)
        {
            // Reset local positions if arrows are children (keeps them centered)
            if (XAxis != null) XAxis.transform.localPosition = Vector3.zero;
            if (YAxis != null) YAxis.transform.localPosition = Vector3.zero;
            if (ZAxis != null) ZAxis.transform.localPosition = Vector3.zero;
        }
    }

    private void Update()
    {
        if (!_arrowActive || _gun == null || _gun.CurrentGrabbedTransform == null) return;

        // Use the directions provided by the gun and the grabbed transform
        SetArrowPos(_gun.CurrentUp, _gun.CurrentRight, _gun.CurrentForward, _gun.CurrentGrabbedTransform);
    }

    private void SetArrowsActive(bool active)
    {
        if (XAxis != null) XAxis.SetActive(active);
        if (YAxis != null) YAxis.SetActive(active);
        if (ZAxis != null) ZAxis.SetActive(active);
    }

    private void SetArrowPos(Vector3 up, Vector3 right, Vector3 forward, Transform t)
    {
        if (t == null) return;

        Vector3 center = t.position;

        if (XAxis != null)
        {
            XAxis.transform.position = center;
            if (right.sqrMagnitude > 1e-6f && up.sqrMagnitude > 1e-6f)
                XAxis.transform.rotation = Quaternion.LookRotation(right.normalized, up.normalized);
        }

        if (YAxis != null)
        {
            YAxis.transform.position = center;
            // For Y arrow, aim its forward along up, with forward vector as a secondary up direction
            if (up.sqrMagnitude > 1e-6f && forward.sqrMagnitude > 1e-6f)
                YAxis.transform.rotation = Quaternion.LookRotation(up.normalized, forward.normalized);
        }

        if (ZAxis != null)
        {
            ZAxis.transform.position = center;
            if (forward.sqrMagnitude > 1e-6f && up.sqrMagnitude > 1e-6f)
                ZAxis.transform.rotation = Quaternion.LookRotation(forward.normalized, up.normalized);
        }
    }
}
