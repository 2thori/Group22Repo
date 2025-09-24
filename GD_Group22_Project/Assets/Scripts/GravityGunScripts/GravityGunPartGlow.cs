using UnityEngine;

public class GravityGunPartGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private Color glowColor = Color.cyan;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private bool glowWhenNearWorkbench = true;
    
    private Renderer partRenderer;
    private Material[] originalMaterials;
    private Material[] glowMaterials;
    private bool isGlowing = false;
    private float currentIntensity = 1f;
    
    // URP specific properties
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    
    // Reference to the workbench
    private GravityGunWorkbench workbench;
    private float workbenchCheckDistance = 5f;

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
                // Create instance materials for glowing
                glowMaterials[i] = new Material(originalMaterials[i]);
            }
            
            partRenderer.materials = glowMaterials;
        }
        
        // Find the workbench in the scene
        workbench = FindObjectOfType<GravityGunWorkbench>();
    }

    private void Update()
    {
        if (isGlowing && glowMaterials != null)
        {
            // Pulsing effect
            currentIntensity = Mathf.PingPong(Time.time * pulseSpeed, maxIntensity - minIntensity) + minIntensity;
            Color currentGlowColor = glowColor * currentIntensity;
            
            foreach (Material mat in glowMaterials)
            {
                if (mat != null)
                {
                    // Set emission color for URP
                    mat.SetColor(EmissionColor, currentGlowColor);
                    
                    // Enable emission
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
        
        // Auto-detect if near workbench
        if (glowWhenNearWorkbench && workbench != null)
        {
            float distance = Vector3.Distance(transform.position, workbench.transform.position);
            if (distance <= workbenchCheckDistance && !isGlowing)
            {
                StartGlowing();
            }
            else if (distance > workbenchCheckDistance && isGlowing)
            {
                StopGlowing();
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
                    // Turn off emission
                    mat.SetColor(EmissionColor, Color.black);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up the material instances
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
    
    // Visualize the glow range in the editor
    private void OnDrawGizmosSelected()
    {
        if (glowWhenNearWorkbench)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, workbenchCheckDistance);
        }
    }
}