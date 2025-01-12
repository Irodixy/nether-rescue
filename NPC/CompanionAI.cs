using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CompanionAI : MonoBehaviour
{
    [Header("Following Settings")]
    [SerializeField] private float minFollowDistance = 2f;
    [SerializeField] private float maxFollowDistance = 4f;
    [SerializeField] private float updatePathInterval = 0.2f;
    [SerializeField] private float heightCheckRadius = 0.5f;

    [Header("Special Movement")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpDetectionRange = 2f;
    [SerializeField] private float jumpableHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float normalHeight = 1.55f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // For more precise ground checking
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundedOffset = 0.1f;

    [Header("Agent Settings")]
    [SerializeField] private float agentHeight = 1.55f;
    [SerializeField] private float agentRadius = 0.5f;
    [SerializeField] private float agentBaseOffset = 0f; // Distance from pivot to bottom of agent

    // Components
    private NavMeshAgent agent;
    private Transform player;
    private Vector3 velocity;
    private CharacterController characterController;
    private bool isGrounded;
    private CompanionState currentState;
    private Vector3 lastPlayerPosition;
    private float timeSinceLastPathUpdate;

    // State tracking
    private enum CompanionState
    {
        Following,
        Jumping,
        Crouching,
        WaitingForPlayer
    }
    //private CompanionState currentState;
    //private bool isGrounded;
    private Vector3 verticalVelocity;

    private void Start()
    {
        InitializeComponents();
        SetupAgent();
        AlignWithNavMesh();
    }

    private void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        characterController = GetComponent<CharacterController>();
        //animator = GetComponent<Animator>();

        if (player == null)
            Debug.LogError("No player found with tag 'Player'");

        lastPlayerPosition = player.position;
    }

    private void SetupAgent()
    {
        // Ensure NavMeshAgent and CharacterController dimensions match
        agent.height = agentHeight;
        agent.radius = agentRadius;
        agent.baseOffset = agentBaseOffset;

        characterController.height = agentHeight;
        characterController.radius = agentRadius;
        characterController.center = new Vector3(0, agentHeight / 2f, 0);

        agent.stoppingDistance = minFollowDistance;
        agent.updateRotation = true;
        agent.updatePosition = false;
    }

    private void AlignWithNavMesh()
    {
        // Sample the nearest valid position on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            // Set the position directly, accounting for agent height
            transform.position = hit.position + Vector3.up * agentBaseOffset;
        }
        else
        {
            Debug.LogError("Could not find valid NavMesh position for companion!");
        }
    }

    private void Update()
    {
        if (player == null) return;

        UpdateGroundedState();
        HandleStateUpdate();
        UpdateAnimation();

        // Apply NavMeshAgent movement through CharacterController
        if (agent.enabled && currentState != CompanionState.Jumping)
        {
            Vector3 movement = agent.desiredVelocity;
            characterController.Move(movement * Time.deltaTime);

            // Update agent's position to match actual position
            agent.nextPosition = transform.position;
        }
    }

    private void UpdateGroundedState()
    {
        // Use a spherecast to check if grounded
        /*isGrounded = Physics.SphereCast(
            transform.position + Vector3.up * 0.1f,
            0.4f,
            Vector3.down,
            out RaycastHit hit,
            0.2f,
            obstacleLayer
        );*/

        /*bool isCurrentlyGrounded = Physics.SphereCast(
            groundCheckTransform.position,
            groundCheckRadius,
            Vector3.down,
            out RaycastHit hit,
            0.1f,
            groundLayer
        );
        Debug.Log(isCurrentlyGrounded);*/

        // More reliable ground check using multiple raycasts
        isGrounded = false;
        float rayLength = 0.3f;
        Vector3 characterBottom = transform.position + Vector3.up * 0.1f;

        // Cast rays in a small circle for better ground detection
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI / 4f;
            Vector3 rayStart = characterBottom + new Vector3(Mathf.Cos(angle) * 0.3f, 0f, Mathf.Sin(angle) * 0.3f);
            if (Physics.Raycast(rayStart, Vector3.down, rayLength, obstacleLayer))
            {
                isGrounded = true;
                break;
            }
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to maintain grounding
        }

        /*Vector3 spherePosition = groundCheck.position + Vector3.up * groundedOffset;
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to maintain grounding
        }*/
    }

    private void HandleStateUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Update path to player periodically
        timeSinceLastPathUpdate += Time.deltaTime;
        if (timeSinceLastPathUpdate >= updatePathInterval)
        {
            UpdatePathToPlayer();
            timeSinceLastPathUpdate = 0f;
        }

        switch (currentState)
        {
            case CompanionState.Following:
                HandleFollowingState(distanceToPlayer);
                break;
            case CompanionState.Jumping:
                HandleJumpingState();
                break;
            case CompanionState.Crouching:
                HandleCrouchingState();
                break;
            case CompanionState.WaitingForPlayer:
                HandleWaitingState(distanceToPlayer);
                break;
        }
    }

    private void HandleFollowingState(float distanceToPlayer)
    {
        // Check if we need to jump
        if (ShouldJump())
        {
            StartJump();
            return;
        }

        // Check if we need to crouch
        if (ShouldCrouch())
        {
            StartCrouch();
            return;
        }

        // Handle normal following
        if (distanceToPlayer > maxFollowDistance)
        {
            currentState = CompanionState.WaitingForPlayer;
            return;
        }

        // Update navigation if player has moved significantly
        if (Vector3.Distance(lastPlayerPosition, player.position) > 1f)
        {
            UpdatePathToPlayer();
            lastPlayerPosition = player.position;
        }
    }

    private void HandleJumpingState()
    {
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Move the agent
        transform.position += velocity * Time.deltaTime;

        if (isGrounded && velocity.y < 0)
        {
            currentState = CompanionState.Following;
            agent.enabled = true;
            return;
        }

        // Apply gravity and move
        //verticalVelocity.y += Physics.gravity.y * Time.deltaTime;
        //characterController.Move(verticalVelocity * Time.deltaTime);
    }

    private void HandleCrouchingState()
    {
        if (!ShouldCrouch())
        {
            EndCrouch();
            return;
        }

        // Continue following while crouched
        UpdatePathToPlayer();
    }

    private void HandleWaitingState(float distanceToPlayer)
    {
        if (distanceToPlayer <= maxFollowDistance)
        {
            currentState = CompanionState.Following;
            UpdatePathToPlayer();
        }
    }

    private bool ShouldJump()
    {
        if (!isGrounded) return false;

        // Cast a ray forward to detect obstacles that require jumping
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, jumpDetectionRange, obstacleLayer))
        {
            // Check if obstacle height requires jumping
            float obstacleHeight = hit.point.y - transform.position.y;
            return obstacleHeight > 0.5f && obstacleHeight < 2f;
        }

        // Check for gaps
        Vector3 forwardPoint = transform.position + transform.forward * jumpDetectionRange;
        return IsJumpableGap(transform.position, forwardPoint);
    }

    private bool IsJumpableGap(Vector3 start, Vector3 end)
    {
        // Check if there's a significant height difference or gap
        if (!NavMesh.SamplePosition(end, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            // No valid NavMesh position found - might be a gap
            if (Physics.Raycast(start, (end - start).normalized, out RaycastHit rayHit, jumpDetectionRange, groundLayer))
            {
                float heightDiff = Mathf.Abs(rayHit.point.y - start.y);
                return heightDiff <= jumpableHeight;
            }
            return true;
        }
        return false;
    }

    private void StartJump()
    {
        currentState = CompanionState.Jumping;
        agent.enabled = false;
        verticalVelocity = new Vector3(agent.velocity.x, jumpForce, agent.velocity.z);
        animator?.SetTrigger("Jump");
    }

    private bool ShouldCrouch()
    {
        // Cast a ray upward to detect low ceilings
        RaycastHit hit;
        return Physics.SphereCast(
            transform.position,
            heightCheckRadius,
            Vector3.up,
            out hit,
            normalHeight,
            obstacleLayer
        );
    }

    private void StartCrouch()
    {
        currentState = CompanionState.Crouching;
        characterController.height = crouchHeight;
        animator?.SetBool("IsCrouching", true);
    }

    private void EndCrouch()
    {
        currentState = CompanionState.Following;
        characterController.height = normalHeight;
        animator?.SetBool("IsCrouching", false);
    }

    private void UpdatePathToPlayer()
    {
        if (agent.enabled && currentState != CompanionState.Jumping)
        {
            agent.SetDestination(player.position);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Update movement animation
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", isGrounded);
    }

    // Visualization for debugging
    private void OnDrawGizmos()
    {
        // Visualize agent dimensions
        Gizmos.color = Color.yellow;
        DrawWireCapsule(
            transform.position + Vector3.up * (agentHeight / 2f),
            transform.position + Vector3.up * (agentHeight / 2f),
            agentRadius,
            agentHeight
        );

        // Visualize ground check rays
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 characterBottom = transform.position + Vector3.up * 0.1f;
            float rayLength = 0.3f;

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                Vector3 rayStart = characterBottom + new Vector3(Mathf.Cos(angle) * 0.3f, 0f, Mathf.Sin(angle) * 0.3f);
                Gizmos.DrawLine(rayStart, rayStart + Vector3.down * rayLength);
            }
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public static void DrawWireCapsule(Vector3 start, Vector3 end, float radius, float height)
    {
        // Draw top and bottom spheres
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);

        // Calculate the directions for the cylinder sides
        Vector3 up = (end - start).normalized;
        Vector3 forward = Vector3.Cross(up, Vector3.right).normalized * radius;
        Vector3 right = Vector3.Cross(up, Vector3.forward).normalized * radius;

        // Draw lines connecting the two spheres
        Gizmos.DrawLine(start + forward, end + forward);
        Gizmos.DrawLine(start - forward, end - forward);
        Gizmos.DrawLine(start + right, end + right);
        Gizmos.DrawLine(start - right, end - right);
    }
}
