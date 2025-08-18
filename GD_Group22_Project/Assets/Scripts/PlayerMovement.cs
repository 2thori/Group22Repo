using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalSpeed = 5f;
    public float crouchSpeed = 2f;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Crouch Settings")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Camera")]
    public Transform playerCamera;
    public float standCamHeight = 1.75f;
    public float crouchCamHeight = 1.0f;
    public float camLerpSpeed = 10f;
    public float mouseSensitivity = 100f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching = false;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.height = standingHeight;
        controller.center = new Vector3(0, standingHeight / 2f, 0);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
        AdjustCameraHeight();
    }

    void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = isCrouching ? crouchSpeed : normalSpeed;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                controller.height = crouchingHeight;
                controller.center = new Vector3(0, crouchingHeight / 2f, 0);
            }
            else
            {
                controller.height = standingHeight;
                controller.center = new Vector3(0, standingHeight / 2f, 0);
            }
        }
    }

    void AdjustCameraHeight()
    {
        float targetY = isCrouching ? crouchCamHeight : standCamHeight;
        Vector3 camPos = playerCamera.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * camLerpSpeed);
        playerCamera.localPosition = camPos;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
