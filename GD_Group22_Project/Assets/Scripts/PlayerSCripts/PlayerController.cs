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

    private AudioSource audioSource;
    private float cameraPitch = 0.0f;
    private CharacterController controller = null;
    private Vector3 velocity;
    
    // Footstep variables
    private bool isMoving = false;
    private bool wasMoving = false;
    private float stepTimer = 0f;
    private Vector2 lastInputDir = Vector2.zero;

    void Start()
    {
        //Pause functionality attempt
        //pMenu.enabled = false;
        
        controller = GetComponent<CharacterController>();
        
        // Initialize audio source with more robust setup
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
        
        // Log audio setup
        Debug.Log($"AudioSource initialized: {audioSource != null}");
        Debug.Log($"Footstep sounds assigned: {footstepSounds != null && footstepSounds.Length > 0}");
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            Debug.Log($"First footstep sound: {footstepSounds[0]}");
        }
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.PausedGame)
            return;

        UpdateMouseLook();
        UpdateMovement();
        UpdateFootstepSounds();
        
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
        controller.Move(move * walkSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateFootstepSounds()
    {
        // Debug movement state occasionally
        if (Random.Range(0, 500) < 1) // Roughly every 500 frames
        {
            Debug.Log($"Moving: {isMoving}, Grounded: {controller.isGrounded}, AudioSource: {audioSource != null}");
        }

        if (wasMoving && !isMoving)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Stopped footstep sound - no longer moving");
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
        
        Debug.Log($"Playing footstep sound: {clipToPlay.name}");
    }
}