using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform playerCamera = null;
    [SerializeField] float mouseSensitivity = 3.5f;
    [SerializeField] float walkSpeed = 6f;
    [SerializeField] float gravity = -9.81f;

    [SerializeField] bool lockCursor = true;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float soundVolume = 0.7f;

    [Header("Gun System")]
    [SerializeField] private Transform gunPivot = null;
    [SerializeField] private GameObject gunModel = null;
    [SerializeField] private float gunFollowSpeed = 8f;
    [SerializeField] private Vector3 gunOffset = new Vector3(0.3f, -0.2f, 0.5f);
    [SerializeField] private KeyCode toggleGunKey = KeyCode.G;

    private AudioSource audioSource;
    private float cameraPitch = 0.0f;
    private CharacterController controller = null;
    private Vector3 velocity;
    
    // Footstep variables
    private bool isMoving = false;
    private bool wasMoving = false;
    private float stepTimer = 0f;
    private Vector2 lastInputDir = Vector2.zero;

    // Gun system variables
    private bool isHoldingGun = false;
    private Vector3 currentGunRotation;

    void Start()
    {
        // Remove the conflict markers and choose one version:
        controller = GetComponent<CharacterController>();
        
        // Initialize audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource component added to player");
        }
        
        // Configure AudioSource
        audioSource.spatialBlend = 1f;
        audioSource.volume = soundVolume;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // Initialize gun system
        InitializeGunSystem();
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void InitializeGunSystem()
    {
        // Create gun pivot if not assigned
        if (gunPivot == null)
        {
            GameObject pivotObject = new GameObject("GunPivot");
            gunPivot = pivotObject.transform;
            gunPivot.SetParent(transform);
            Debug.Log("Created GunPivot automatically");
        }

        // Disable gun model at start
        if (gunModel != null)
        {
            gunModel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No gun model assigned to PlayerController");
        }

        currentGunRotation = Vector3.zero;
    }

    void Update()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.PausedGame)
            return;

        UpdateMouseLook();
        UpdateMovement();
        UpdateFootstepSounds();
        UpdateGunInput();
        UpdateGunPosition();
    }

    void UpdateMouseLook()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        cameraPitch -= mouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);
    }

    void UpdateMovement()
    {
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        inputDir.Normalize();

        wasMoving = isMoving;
        isMoving = inputDir.magnitude > 0.1f && controller.isGrounded;
        lastInputDir = inputDir;

        Vector3 move = (transform.forward * inputDir.y + transform.right * inputDir.x);
        
        // Adjust speed if holding gun
        float currentSpeed = isHoldingGun ? walkSpeed * 0.8f : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateFootstepSounds()
    {
        if (wasMoving && !isMoving)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        if (!isMoving || !controller.isGrounded)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= walkStepInterval)
        {
            PlayRandomFootstep();
            stepTimer = 0f;
        }
    }

    void PlayRandomFootstep()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null!");
            return;
        }

        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            Debug.LogError("No footstep sounds assigned!");
            return;
        }

        AudioClip clipToPlay = footstepSounds[Random.Range(0, footstepSounds.Length)];
        
        if (clipToPlay == null)
        {
            Debug.LogError("Selected footstep AudioClip is null!");
            return;
        }

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clipToPlay, soundVolume);
    }

    void UpdateGunInput()
    {
        // Toggle gun with G key
        if (Input.GetKeyDown(toggleGunKey))
        {
            ToggleGun();
        }

        // You can also add other gun-related input here
        // For example, using the gravity gun:
        if (isHoldingGun && Input.GetMouseButtonDown(0))
        {
            UseGravityGun();
        }
    }

    void UpdateGunPosition()
    {
        if (!isHoldingGun || gunPivot == null || playerCamera == null) 
            return;

        // Make gun follow camera rotation smoothly
        Vector3 cameraRotation = playerCamera.eulerAngles;
        
        // Convert to -180 to 180 range for smooth interpolation
        float cameraPitchNormalized = cameraRotation.x;
        if (cameraPitchNormalized > 180f) 
            cameraPitchNormalized -= 360f;
        
        // Smoothly interpolate gun rotation
        currentGunRotation = Vector3.Lerp(currentGunRotation, 
                                        new Vector3(cameraPitchNormalized, cameraRotation.y, 0f), 
                                        gunFollowSpeed * Time.deltaTime);
        
        gunPivot.rotation = Quaternion.Euler(currentGunRotation);

        // Position gun relative to camera
        Vector3 targetPosition = playerCamera.position + 
                               playerCamera.forward * gunOffset.z +
                               playerCamera.right * gunOffset.x +
                               playerCamera.up * gunOffset.y;
        
        gunPivot.position = Vector3.Lerp(gunPivot.position, targetPosition, gunFollowSpeed * Time.deltaTime);
    }

    void ToggleGun()
    {
        isHoldingGun = !isHoldingGun;
        
        if (gunModel != null)
        {
            gunModel.SetActive(isHoldingGun);
            Debug.Log($"Gun {(isHoldingGun ? "equipped" : "unequipped")}");
        }

        // You can add sound effects here for equip/unequip
        // PlayGunEquipSound(isHoldingGun);
    }

    void UseGravityGun()
    {
        // Add your gravity gun functionality here
        Debug.Log("Gravity gun used!");
        
        // Example: Shoot raycast to detect objects
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Debug.Log($"Gravity gun hit: {hit.transform.name}");
            
            // Add your gravity gun effects here
            // For example, apply force to rigidbodies
            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(playerCamera.forward * 10f, ForceMode.Impulse);
            }
        }
    }

    // Public methods for other scripts to control the gun
    public void EquipGun()
    {
        if (!isHoldingGun)
        {
            ToggleGun();
        }
    }

    public void UnequipGun()
    {
        if (isHoldingGun)
        {
            ToggleGun();
        }
    }

    public bool IsHoldingGun()
    {
        return isHoldingGun;
    }

    // Method to call when player picks up the gravity gun item
    public void OnPickupGravityGun()
    {
        EquipGun();
        Debug.Log("Picked up gravity gun!");
    }

    // Optional: Adjust gun position at runtime
    public void SetGunOffset(Vector3 newOffset)
    {
        gunOffset = newOffset;
    }

    public void SetGunFollowSpeed(float speed)
    {
        gunFollowSpeed = speed;
    }
}