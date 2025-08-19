using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform playerCamera = null;
    [SerializeField] float mouseSensitivity = 3.5f;
    [SerializeField] float walkSpeed = 6f;
    [SerializeField] float gravity = -9.81f;

    [SerializeField] bool lockCursor = true;

    float cameraPitch = 0.0f;
    CharacterController controller = null;

    Vector3 velocity; // stores vertical motion (gravity + jump later)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        UpdateMouseLook();
        UpdateMovement();
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
        // Get WASD input
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        inputDir.Normalize();

        // Horizontal movement
        Vector3 move = (transform.forward * inputDir.y + transform.right * inputDir.x);
        controller.Move(move * walkSpeed * Time.deltaTime);

        // ✅ Gravity fix
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // keep player grounded (instead of floating)
        }

        velocity.y += gravity * Time.deltaTime; // apply gravity over time
        controller.Move(velocity * Time.deltaTime); // apply vertical velocity
    }
}