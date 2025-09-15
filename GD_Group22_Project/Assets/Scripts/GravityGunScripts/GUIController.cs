using UnityEngine;
using TMPro;

public class GUIController : MonoBehaviour
{
    [Header("UI (TextMeshPro)")]
    [SerializeField] private TMP_Text rotationText;

    [Header("Behaviour")]
    [Tooltip("If true the UI will be hidden when rotation snapping is off.")]
    [SerializeField] private bool hideWhenNotSnapped = true;

    private PhysicsGunInteractionBehavior _physicsGun;
    private bool _objectAxis;

    private void Awake()
    {
        // auto-find if not assigned
        if (rotationText == null)
            rotationText = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnEnable()
    {
        if (_physicsGun == null)
            _physicsGun = FindObjectOfType<PhysicsGunInteractionBehavior>();

        if (_physicsGun != null)
        {
            _physicsGun.OnRotationSnapped.AddListener(OnRotationSnapped);
            _physicsGun.OnAxisChanged.AddListener(OnAxisChange);
        }

        UpdateText();
        SetVisible(!hideWhenNotSnapped);
    }

    private void OnDisable()
    {
        if (_physicsGun != null)
        {
            _physicsGun.OnRotationSnapped.RemoveListener(OnRotationSnapped);
            _physicsGun.OnAxisChanged.RemoveListener(OnAxisChange);
        }
    }

    private void OnAxisChange(bool axis)
    {
        _objectAxis = axis;
        UpdateText();
    }

    private void OnRotationSnapped(bool snapped)
    {
        SetVisible(snapped || !hideWhenNotSnapped);
        UpdateText();
    }

    private void UpdateText()
    {
        if (rotationText == null) return;

        rotationText.text = $"Snapped Axis = {(_objectAxis ? "Object Axis" : "Player Axis")}";
    }

    private void SetVisible(bool visible)
    {
        if (rotationText == null) return;
        rotationText.gameObject.SetActive(visible);
    }
}