using UnityEngine;
using UnityEngine.SceneManagement;

public class MovimentarJogador : MonoBehaviour
{
    const float VELOCIDADE_BASE = 5f;

    [Header("Movement Settings")]
    [SerializeField] private float normalSpeed = VELOCIDADE_BASE;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float rotationSmoothTime = 0.15f;
    [SerializeField] private float minimumMovementThreshold = 0.1f;

    [Header("Power-up Multipliers")]
    public float speedMultiplier = 1f;
    public float jumpMultiplier = 1f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float crouchVerticalOffset = 0.4f; // How much to move down when crouching

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject modelPlayer;
    [SerializeField] private float animationBlendSpeed = 10f;

    // Animation parameter hashes for better performance
    private readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
    private readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int JumpHash = Animator.StringToHash("Jump");
    private readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

    [Header("Camera Settings")]
    [SerializeField] private Transform mainCamera;
    private Vector3 cameraOffset; // Default camera position relative to player
    [SerializeField] private float cameraFollowSpeed = 5f;
    [SerializeField] private float cameraTransitionSpeed = 2f;

    [Header("Camera Zone Settings")]
    [SerializeField] private bool isInCameraZone = false;
    private CameraSettings currentZoneSettings;
    private CameraSettings defaultCameraSettings;

    // For more precise ground checking
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Physics")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 3f;

    // Private variables
    private CharacterController controller;
    private Vector3 movement;
    private float currentSpeed;
    private float rotationSmoothVelocity;
    private Vector3 verticalVelocity;
    private Quaternion targetRotation;
    private float currentMovementSpeed; // For animation blending
    private string currentScene;

    private void Start()
    {
        // Subscribe to sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        currentScene = SceneManager.GetActiveScene().name;

        InitializeComponents();
        SetupCamera(currentScene);
        SetupCursor();
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reconnect camera when a new scene is loaded
        SetupCamera(scene.name);
    }

    private void InitializeComponents()
    {
        controller = GetComponent<CharacterController>();
        // Initial animator setup
        if (animator == null)
            animator = GetComponent<Animator>();

        currentSpeed = normalSpeed;
    }

    private void SetupCamera(string scene)
    {
        // Initial camera setup
        if (mainCamera == null)
        {
            GameObject cameraObject = GameObject.Find("Main Camera");
            if (cameraObject != null)
            {
                mainCamera = cameraObject.GetComponent<Camera>().transform;
            }
        }

        // Initialize default camera settings base on scene
        if (scene == "TutorialScene")
        {
            defaultCameraSettings = CameraData(new Vector3(0, 2, -5), new Vector3(20, 0, 0));
        }
        else if (scene == "MapFase1Part1")
        {
            defaultCameraSettings = CameraData(new Vector3(0, 2, 5), new Vector3(20, 180, 0));
        }
        else if (scene == "MapFase1Part2")
        {
            defaultCameraSettings = CameraData(new Vector3(0, 2, -5), new Vector3(20, 0, 0));
        }
        else if (scene == "MapFase1Part3")
        {
            defaultCameraSettings = CameraData(new Vector3(0, 2, -5), new Vector3(20, 0, 0));
        }

        currentZoneSettings = defaultCameraSettings;
        // Set initial camera position
        UpdateCameraPosition();
    }

    private CameraSettings CameraData(Vector3 position, Vector3 rotation)
    {
        return new CameraSettings
        {
            zoneName = "Default",
            cameraOffset = position,
            cameraRotation = rotation,
            transitionSpeed = cameraTransitionSpeed
        };
    }

    private void SetupCursor()
    {
        // Hide and lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (PauseManager.Instance.IsPaused)
            return;

        HandleMovement();
        HandleGravity();
        UpdateAnimations();
        UpdateCameraPosition();
    }

    private void HandleMovement()
    {
        // Get raw input
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Only process movement if there's input
        if (input.magnitude >= minimumMovementThreshold)
        {
            // Normalize input
            input = Vector2.ClampMagnitude(input, 1f);

            // Determine movement direction relative to camera view
            Vector3 cameraForward = mainCamera.forward;
            Vector3 cameraRight = mainCamera.right;

            // Project vectors onto horizontal plane
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate desired move direction
            Vector3 moveDirection = (cameraRight * input.x + cameraForward * input.y).normalized;

            // Update movement speed based on input
            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed :
                         Input.GetKey(KeyCode.LeftControl) ? crouchSpeed :
                         normalSpeed;

            // Set movement and rotation
            movement = moveDirection * (currentSpeed * speedMultiplier);
            targetRotation = Quaternion.LookRotation(moveDirection);

            // Smooth rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime);

            // Update animation blend value
            currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, input.magnitude, Time.deltaTime * animationBlendSpeed);
        }
        else
        {
            // Immediately stop movement when no input
            movement = Vector3.zero;
            currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, 0f, Time.deltaTime * animationBlendSpeed);
        }

        // Crouch logic
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        animator.SetBool(IsCrouchingHash, isCrouching);

        // Dynamic height adjustment
        AdjustPlayerVerticalPosition(isCrouching);

        // Apply movement
        controller.Move((movement + verticalVelocity) * Time.deltaTime);
    }

    private void HandleGravity()
    {
        /*bool wasGrounded = controller.isGrounded;

        if (controller.isGrounded)
        {
            verticalVelocity.y = -2f; // Small constant downward force

            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                animator.SetTrigger(JumpHash);
            }
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        // Update grounded state in animator only when it changes
        if (wasGrounded != controller.isGrounded)
        {
            animator.SetBool(IsGroundedHash, controller.isGrounded);
        }*/

        // Use spherecast for more reliable ground detection
        bool isCurrentlyGrounded = Physics.SphereCast(
            groundCheckTransform.position,
            groundCheckRadius,
            Vector3.down,
            out RaycastHit hit,
            0.1f,
            groundLayer
        );

        // Reset jump trigger after landing
        if (isCurrentlyGrounded)
        {
            verticalVelocity.y = -2f; // Slight downward force to stick to ground

            // Reset jump state when grounded
            if (!controller.isGrounded)
            {
                animator.ResetTrigger(JumpHash);
                //animator.SetTrigger("Land");
            }
        }
        else
        {
            // Apply gravity when not grounded
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        // Update animator ground state
        animator.SetBool(IsGroundedHash, isCurrentlyGrounded);

        // Jump logic
        if (isCurrentlyGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            // Ensure jump trigger is reset before setting
            animator.ResetTrigger(JumpHash);
            animator.SetTrigger(JumpHash);

            verticalVelocity.y = Mathf.Sqrt((jumpForce * jumpMultiplier) * -1f * gravity);
        }
    }

    private void UpdateAnimations()
    {
        animator.SetFloat(MovementSpeedHash, currentMovementSpeed);
        animator.SetFloat(VerticalVelocityHash, verticalVelocity.y);
    }

    private void UpdateCameraPosition()
    {
        // Null checks to prevent potential errors
        if (mainCamera == null)
        {
            Debug.LogError("Main camera is not assigned!");
            return;
        }

        Vector3 targetPosition;
        Quaternion targetRotation;
        float currentTransitionSpeed;

        try
        {
            if (isInCameraZone)
            {
                targetPosition = transform.position + currentZoneSettings.cameraOffset;
                targetRotation = Quaternion.Euler(currentZoneSettings.cameraRotation);
                currentTransitionSpeed = currentZoneSettings.transitionSpeed;
            }
            else
            {
                targetPosition = transform.position + defaultCameraSettings.cameraOffset;
                targetRotation = Quaternion.Euler(defaultCameraSettings.cameraRotation);
                currentTransitionSpeed = defaultCameraSettings.transitionSpeed;
            }

            mainCamera.position = Vector3.Lerp(
                mainCamera.position,
                targetPosition,
                Time.deltaTime * currentTransitionSpeed
            );

            mainCamera.rotation = Quaternion.Lerp(
                mainCamera.rotation,
                targetRotation,
                Time.deltaTime * currentTransitionSpeed
            );
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Camera position update failed: {e.Message}");
        }
    }

    private void AdjustPlayerVerticalPosition(bool isCrouching)
    {
        if (isCrouching)
        {
            // Lower character controller height
            controller.height = crouchHeight;

            // Move character's model transform down to prevent floating
            Vector3 newPosition = modelPlayer.transform.position;
            //modelPlayer
            newPosition.y -= crouchVerticalOffset;
            transform.position = newPosition;

            // Optional: Adjust ground check transform
            /*if (groundCheckTransform != null)
            {
                Vector3 groundCheckPosition = groundCheckTransform.localPosition;
                groundCheckPosition.y -= crouchVerticalOffset;
                groundCheckTransform.localPosition = groundCheckPosition;
            }*/
        }
        else
        {
            // Restore original height
            controller.height = standingHeight;

            // Move character's transform back up
            Vector3 newPosition = modelPlayer.transform.position;
            newPosition.y += crouchVerticalOffset;
            transform.position = newPosition;

            // Restore ground check transform
            /*if (groundCheckTransform != null)
            {
                Vector3 groundCheckPosition = groundCheckTransform.localPosition;
                groundCheckPosition.y += crouchVerticalOffset;
                groundCheckTransform.localPosition = groundCheckPosition;
            }*/
        }
    }

    // Called by trigger zones to change camera behavior
    public void EnterCameraZone(CameraSettings settings)
    {
        isInCameraZone = true;
        currentZoneSettings = settings;
    }

    public void ExitCameraZone()
    {
        isInCameraZone = false;
        currentZoneSettings = defaultCameraSettings;
    }

    // Visual debug for ground check
    private void OnDrawGizmosSelected()
    {
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }
    }
}