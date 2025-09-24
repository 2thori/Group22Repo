using UnityEngine;

public class GravityGunPartGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private Color glowColor = Color.cyan;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 2f;

    private Renderer partRenderer;
    private Material[] originalMaterials;
    private Material[] glowMaterials;
    private bool isGlowing = false;
    private float currentIntensity = 1f;

    // URP shader property IDs
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        partRenderer = GetComponent<Renderer>();
        if (partRenderer != null)
        {
            // Store original materials and create glow material instances
            originalMaterials = partRenderer.materials;
            glowMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                glowMaterials[i] = new Material(originalMaterials[i]);
            }

            partRenderer.materials = glowMaterials;
        }
    }

    private void Update()
    {
        if (isGlowing && glowMaterials != null)
        {
            // Pulsing intensity between min and max
            currentIntensity = Mathf.PingPong(Time.time * pulseSpeed, maxIntensity - minIntensity) + minIntensity;
            Color currentGlowColor = glowColor * currentIntensity;

            foreach (Material mat in glowMaterials)
            {
                if (mat != null)
                {
                    mat.SetColor(EmissionColor, currentGlowColor);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    public void StartGlowing()
    {
        isGlowing = true;
    }

    public void StopGlowing()
    {
        isGlowing = false;

        if (glowMaterials != null)
        {
            foreach (Material mat in glowMaterials)
            {
                if (mat != null)
                {
                    mat.SetColor(EmissionColor, Color.black);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (glowMaterials != null)
        {
            foreach (Material mat in glowMaterials)
            {
                if (mat != null)
                {
                    DestroyImmediate(mat);
                }
            }
        }
    }
}
