using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PulseColor : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("The color to pulse towards")]
    public Color pulseColor = Color.cyan;

    [Tooltip("Speed of pulsing")]
    public float pulseSpeed = 2f;

    [Tooltip("Minimum emission intensity")]
    public float minIntensity = 0.5f;

    [Tooltip("Maximum emission intensity")]
    public float maxIntensity = 2f;

    private Renderer objRenderer;
    private Material[] materials;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            Material[] originalMats = objRenderer.materials;
            materials = new Material[originalMats.Length];

            for (int i = 0; i < originalMats.Length; i++)
            {
                materials[i] = new Material(originalMats[i]); // instance materials so only this object pulses
                materials[i].EnableKeyword("_EMISSION");     // enable emission
            }

            objRenderer.materials = materials;
        }
    }

    void Update()
    {
        if (materials == null) return;

        // Calculate pulsing intensity
        float intensity = Mathf.PingPong(Time.time * pulseSpeed, maxIntensity - minIntensity) + minIntensity;
        Color currentColor = pulseColor * intensity;

        // Apply to all materials
        foreach (Material mat in materials)
        {
            if (mat != null)
                mat.SetColor(EmissionColorID, currentColor);
        }
    }

    private void OnDestroy()
    {
        // clean up instantiated materials
        if (materials != null)
        {
            foreach (Material mat in materials)
            {
                if (mat != null)
                    DestroyImmediate(mat);
            }
        }
    }
}