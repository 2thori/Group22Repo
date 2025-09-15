using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public class AngleSlider : MonoBehaviour
{
    [Header("UI (TMP preferred)")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Text legacyText;

    [Header("Angle Range")]
    [Tooltip("Minimum allowed snap angle (degrees)")]
    [SerializeField] private float minAngle = 1f;
    [Tooltip("Maximum allowed snap angle (degrees)")]
    [SerializeField] private float maxAngle = 180f;

    private PhysicsGunInteractionBehavior _guncontroller;

    private void Awake()
    {
        // auto-find components if not set
        if (slider == null) slider = GetComponent<Slider>();
        if (tmpText == null && legacyText == null)
        {
            tmpText = GetComponentInChildren<TMP_Text>(true);
            if (tmpText == null)
                legacyText = GetComponentInChildren<Text>(true);
        }
    }

    private void Start()
    {
        if (slider == null)
        {
            Debug.LogWarning($"{nameof(AngleSlider)} requires a Slider component.", this);
            enabled = false;
            return;
        }

        // clamp sensible range
        if (minAngle < 0f) minAngle = 0f;
        if (maxAngle <= minAngle) maxAngle = Mathf.Max(1f, minAngle + 1f);

        slider.minValue = minAngle;
        slider.maxValue = maxAngle;

        _guncontroller = FindObjectOfType<PhysicsGunInteractionBehavior>();

        if (_guncontroller != null)
        {
            // initialize from gun value (clamped)
            slider.value = Mathf.Clamp(_guncontroller.SnapRotationDegrees, minAngle, maxAngle);
            slider.onValueChanged.AddListener(OnSliderUpdated);
            slider.interactable = true;
        }
        else
        {
            // no gun present — disable interaction but allow editing in inspector if needed
            slider.value = (minAngle + maxAngle) * 0.5f;
            slider.interactable = false;
            Debug.LogWarning($"{nameof(AngleSlider)}: No PhysicsGunInteractionBehavior found in scene.", this);
        }

        UpdateText(slider.value);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderUpdated);
    }

    private void OnSliderUpdated(float value)
    {
        if (_guncontroller != null)
        {
            _guncontroller.SnapRotationDegrees = Mathf.Clamp(value, minAngle, maxAngle);
        }

        UpdateText(value);
    }

    private void UpdateText(float value)
    {
        string s = $"Snap: {Mathf.RoundToInt(value)}°";

        if (tmpText != null)
            tmpText.text = s;
        else if (legacyText != null)
            legacyText.text = s;
    }
}
