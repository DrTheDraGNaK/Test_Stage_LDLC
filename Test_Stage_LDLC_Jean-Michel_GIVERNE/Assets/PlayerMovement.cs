using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Camera Controls")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxCameraAngle = 80f;
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float rotationX;
    private bool isGrounded;
    private bool wasGrounded;
    private const float gravity = -9.81f;

    // Input System variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool jumpPressed;
    private bool canJump = true;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Sprint.performed += OnSprintStart;
        inputActions.Player.Sprint.canceled += OnSprintEnd;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();

        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Sprint.performed -= OnSprintStart;
        inputActions.Player.Sprint.canceled -= OnSprintEnd;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!cameraTransform)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        if (isGrounded && !wasGrounded)
        {
            canJump = true;
            jumpPressed = false;
        }

        if (!isGrounded && wasGrounded)
        {
            jumpPressed = false;
        }

        HandleCameraRotation();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void HandleCameraRotation()
    {
        float rotationY = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * rotationY);

        rotationX -= lookInput.y * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -maxCameraAngle, maxCameraAngle);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    private void HandleMovement()
    {
        Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 movement = transform.TransformDirection(direction) * currentSpeed;

        controller.Move(movement * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded && canJump)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity * gravityMultiplier);
            jumpPressed = false;
            canJump = false;
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
        }

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    #region Input System Callbacks
    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    private void OnSprintStart(InputAction.CallbackContext context)
    {
        isRunning = true;
    }

    private void OnSprintEnd(InputAction.CallbackContext context)
    {
        isRunning = false;
    }
    #endregion
}
