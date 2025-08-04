using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 8f;
    public float sprintSpeed = 11f;
    public float jumpForce = 7f;
    public float slideSpeed = 10f;
    public float slideDuration = 1f;
    public float headBobFrequency = 10f;
    public float headBobAmplitude = 0.1f;
    public float mouseSensitivity = 5f;
    public float lookUpLimit = 80f;
    public float lookDownLimit = -80f;

    private CharacterController controller;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool isSliding;
    private float slideTimer;
    private Vector3 initialCameraPosition;
    private float rotationX = 0f;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private Camera playerCameraComponent;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        initialCameraPosition = playerCamera.localPosition;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        HandleMouseLook();

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        inputDirection.Normalize();

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        float targetFOV = currentSpeed == sprintSpeed ? 90f : 60f;

        playerCameraComponent.fieldOfView = Mathf.Lerp(playerCameraComponent.fieldOfView, targetFOV, Time.deltaTime * 8f);

        if (Input.GetKeyDown(KeyCode.C) && !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
            controller.height /= 2f;
            if (!isGrounded)
            {
                moveDirection.y = -3f;
            }
        }

        float verticalVelocity = moveDirection.y;

        if (isSliding)
        {
            moveDirection = transform.forward * slideSpeed;
        }
        else
        {
            moveDirection = inputDirection * currentSpeed;
        }

        moveDirection.y = verticalVelocity;

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                isSliding = false;
                controller.height *= 2f;
            }
        }

        if (Input.GetButtonDown("Jump") && isGrounded && !isSliding)
        {
            moveDirection.y = jumpForce;
        }

        if (!isGrounded)
        {
            moveDirection.y += Physics.gravity.y * Time.deltaTime;
        }

        controller.Move(moveDirection * Time.deltaTime);

        if (controller.velocity.magnitude > 0.1f && isGrounded && !isSliding)
        {
            float bobbingAmount = Mathf.Sin(Time.time * headBobFrequency) * headBobAmplitude;
            playerCamera.localPosition = initialCameraPosition + new Vector3(0, bobbingAmount, 0);
        }
        else
        {
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, initialCameraPosition, Time.deltaTime * 5f);
        }
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        transform.Rotate(Vector3.up * mouseX);
        
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, lookDownLimit, lookUpLimit);
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}