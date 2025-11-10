using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("Animation Layers")]
    [SerializeField] private int baseLayerIndex = 0;
    [SerializeField] private int gunLayerIndex = 1;
    [SerializeField] private int actionsLayerIndex = 2;
    
    [Header("Layer Weights")]
    [SerializeField] private float gunLayerWeight = 1f;
    [SerializeField] private float actionsLayerWeight = 1f;

    private Animator animator;
    private bool isHoldingGun = false;
    private bool isHoldingObject = false;
    private float movementSpeed = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetupAnimationLayers();
    }

    void SetupAnimationLayers()
    {
        // Set up gun layer (additive)
        if (animator.layerCount > gunLayerIndex)
        {
            animator.SetLayerWeight(gunLayerIndex, isHoldingGun ? gunLayerWeight : 0f);
        }
        
        // Set up actions layer (additive)
        if (animator.layerCount > actionsLayerIndex)
        {
            animator.SetLayerWeight(actionsLayerIndex, actionsLayerWeight);
        }
    }

    void Update()
    {
        HandleMovement();
        HandleInput();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        // Get movement input and calculate speed
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        movementSpeed = new Vector3(horizontal, 0, vertical).magnitude;
        
        animator.SetFloat("MovementSpeed", movementSpeed);
    }

    void HandleInput()
    {
        // Toggle gun with G key
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGun();
        }
        
        // Grab/Pickup with E key
        if (Input.GetKeyDown(KeyCode.E))
        {
            GrabPickup();
        }
        
        // Push with F key
        if (Input.GetKeyDown(KeyCode.F))
        {
            Push();
        }
        
        // Press with P key
        if (Input.GetKeyDown(KeyCode.P))
        {
            Press();
        }
    }

    void UpdateAnimator()
    {
        // Update base layer parameters
        animator.SetBool("IsHoldingObject", isHoldingObject);
        
        // Update gun layer weight based on whether holding gun
        if (animator.layerCount > gunLayerIndex)
        {
            float targetWeight = isHoldingGun ? gunLayerWeight : 0f;
            float currentWeight = animator.GetLayerWeight(gunLayerIndex);
            float newWeight = Mathf.Lerp(currentWeight, targetWeight, 8f * Time.deltaTime);
            animator.SetLayerWeight(gunLayerIndex, newWeight);
        }
    }

    public void ToggleGun()
    {
        isHoldingGun = !isHoldingGun;
        Debug.Log("Gun: " + (isHoldingGun ? "Equipped" : "Unequipped"));
    }

    public void GrabPickup()
    {
        // Play on ACTIONS layer - won't interrupt movement or gun
        animator.SetTrigger("Grab");
        
        // Toggle object holding state
        isHoldingObject = !isHoldingObject;
        Debug.Log(isHoldingObject ? "Grabbed object" : "Released object");
    }

    public void Push()
    {
        // Play on ACTIONS layer
        animator.SetTrigger("Push");
        Debug.Log("Pushing");
    }

    public void Press()
    {
        // Play on ACTIONS layer
        animator.SetTrigger("Press");
        Debug.Log("Pressing button");
    }

    // Public methods for other scripts to call
    public void PlayGrabAnimation()
    {
        animator.SetTrigger("Grab");
    }

    public void SetHoldingObject(bool holding)
    {
        isHoldingObject = holding;
    }

    public void SetHoldingGun(bool holding)
    {
        isHoldingGun = holding;
    }

    public bool IsHoldingGun()
    {
        return isHoldingGun;
    }

    public bool IsHoldingObject()
    {
        return isHoldingObject;
    }

    public bool IsMoving()
    {
        return movementSpeed > 0.1f;
    }

    // Layer weight controls
    public void SetGunLayerWeight(float weight)
    {
        gunLayerWeight = Mathf.Clamp01(weight);
    }

    public void SetActionsLayerWeight(float weight)
    {
        actionsLayerWeight = Mathf.Clamp01(weight);
        if (animator.layerCount > actionsLayerIndex)
        {
            animator.SetLayerWeight(actionsLayerIndex, actionsLayerWeight);
        }
    }
}