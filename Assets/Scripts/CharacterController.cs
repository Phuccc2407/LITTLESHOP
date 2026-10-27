using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnityEngine.CharacterController))]
public sealed class CharacterController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform cameraRoot;
    [SerializeField, Min(0f)] private float walkSpeed = 4f;
    [SerializeField, Min(0f)] private float sprintSpeed = 7f;
    [SerializeField, Min(0f)] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gravity = -20f;
    [SerializeField, Min(0f)] private float jumpHeight = 1.2f;

    private UnityEngine.CharacterController controller;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private float verticalVelocity;
    private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<UnityEngine.CharacterController>();
        if (inputActions == null)
        {
            Debug.LogError("CharacterController requires an InputActionAsset.", this);
            return;
        }

        InputActionMap map = inputActions.FindActionMap("Player", true);
        moveAction = map.FindAction("Move", true);
        lookAction = map.FindAction("Look", true);
        jumpAction = map.FindAction("Jump", true);
        sprintAction = map.FindAction("Sprint", true);
    }

    private void OnEnable()
    {
        if (moveAction == null)
        {
            return;
        }

        moveAction.actionMap.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.actionMap.Disable();
        }
    }

    private void Update()
    {
        if (moveAction == null || (GameViewManager.Instance != null && GameViewManager.Instance.IsBlockingViewOpen))
        {
            return;
        }

        UpdateLook();
        UpdateMovement();
    }

    private void UpdateLook()
    {
        if (cameraRoot == null || lookAction == null)
        {
            return;
        }

        Vector2 look = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, look.x * mouseSensitivity);
        cameraPitch = Mathf.Clamp(cameraPitch - look.y * mouseSensitivity, -89f, 89f);
        cameraRoot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void UpdateMovement()
    {
        Vector2 input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        float speed = sprintAction != null && sprintAction.IsPressed() ? sprintSpeed : walkSpeed;
        Vector3 movement = (transform.right * input.x + transform.forward * input.y) * speed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (jumpAction != null && jumpAction.WasPressedThisFrame())
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;
        controller.Move(movement * Time.deltaTime);
    }
}
