using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyStalkerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float normalSpeed = 2.5f;         // Base stalking speed
    [SerializeField] private float retreatSpeed = 4f;          // Faster retreat speed when repelled
    [SerializeField] private float minDistanceToPlayer = 1.5f; // Distance to trigger attack

    [Header("Torch Interaction Settings")]
    [SerializeField] private float maxTorchEffectDistance = 10f;  // Maximum distance torch can affect enemy
    [SerializeField] private float retreatDistanceFromTorch = 6f; // Distance to maintain when affected by torch
    [SerializeField] private float returnDelay = 0.5f;

    [Header("NavMesh Settings")]
    [SerializeField] private float agentRadius = 0.03f;        // Added to control separation
    [SerializeField] private bool avoidOtherAgents = false;   // Toggle for agent avoidance

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // Animation parameter hashes for better performance
    private readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int IsRetreatingHash = Animator.StringToHash("IsRetreating");
    private readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    // Component references
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // State management
    private enum StalkerState { Pursuing, Retreating, Attacking }
    private StalkerState currentState;
    private bool isAffectedByTorch = false;
    private float lastTorchEffectTime;

    private void Start()
    {
        InitializeComponents();
        SaveOriginalTransform();
        SetInitialState();
    }

    private void InitializeComponents()
    {
        // Get or add required components
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        // Configure NavMeshAgent for slow, deliberate movement
        agent.speed = normalSpeed;
        agent.acceleration = 3f;          // Slow acceleration for menacing movement
        agent.angularSpeed = 120f;        // Slower turning for dramatic effect
        agent.stoppingDistance = minDistanceToPlayer;

        // Critical fixes for multi-agent behavior
        agent.radius = agentRadius;
        agent.avoidancePriority = Random.Range(0, 99); // Randomize priority to prevent uniform behavior
        agent.obstacleAvoidanceType = avoidOtherAgents ? ObstacleAvoidanceType.LowQualityObstacleAvoidance
                                                      : ObstacleAvoidanceType.NoObstacleAvoidance;

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void SaveOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void SetInitialState()
    {
        TransitionToState(StalkerState.Pursuing);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Check if torch effect has expired
        if (isAffectedByTorch && Time.time > lastTorchEffectTime + returnDelay)
        {
            isAffectedByTorch = false;
            if (currentState == StalkerState.Retreating)
            {
                TransitionToState(StalkerState.Pursuing);
            }
        }

        switch (currentState)
        {
            case StalkerState.Pursuing:
                HandlePursuit();
                break;

            case StalkerState.Retreating:
                HandleRetreat();
                break;

            case StalkerState.Attacking:
                // Attack state is handled by animation events
                break;
        }
    }

    private void HandlePursuit()
    {
        if (isAffectedByTorch) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= minDistanceToPlayer)
        {
            TransitionToState(StalkerState.Attacking);
            return;
        }

        // Update destination to player position
        agent.SetDestination(playerTransform.position);
    }

    private void HandleRetreat()
    {
        // Calculate retreat position - move away from player while maintaining line of sight
        Vector3 directionFromPlayer = (transform.position - playerTransform.position).normalized;
        Vector3 targetRetreatPosition/*retreatPosition*/ = playerTransform.position + directionFromPlayer * retreatDistanceFromTorch;//torchRetreatDistance;

        // Use NavMesh to find valid retreat position
        //NavMeshHit hit;
        /*if (NavMesh.SamplePosition(retreatPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }*/
        // Find valid position on NavMesh
        if (NavMesh.SamplePosition(targetRetreatPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        // Check if we're far enough to resume pursuit
        float currentDistance = Vector3.Distance(transform.position, playerTransform.position);
        if (currentDistance >= retreatDistanceFromTorch && !isAffectedByTorch)//torchRetreatDistance)
        {
            TransitionToState(StalkerState.Pursuing);
        }
    }

    // Called by the torch's cone detection
    public void RetreatFromTorch(Vector3 torchPosition)
    {
        float distanceToTorch = Vector3.Distance(transform.position, torchPosition);

        if (distanceToTorch <= maxTorchEffectDistance)
        {
            isAffectedByTorch = true;
            lastTorchEffectTime = Time.time;

            if (currentState != StalkerState.Retreating)
            {
                TransitionToState(StalkerState.Retreating);
            }
        }
    }

    // Called by torch system when player aims torch at enemy
    public void OnTorchAimed(bool isAimed)
    {
        if (isAimed && currentState == StalkerState.Pursuing)
        {
            TransitionToState(StalkerState.Retreating);
        }
        else if (!isAimed && currentState == StalkerState.Retreating)
        {
            TransitionToState(StalkerState.Pursuing);
        }
    }

    private void TransitionToState(StalkerState newState)
    {
        // Exit current state
        switch (currentState)
        {
            case StalkerState.Retreating:
                agent.speed = normalSpeed;
                break;
        }

        // Enter new state
        switch (newState)
        {
            case StalkerState.Pursuing:
                agent.isStopped = false;
                agent.speed = normalSpeed;
                animator.SetBool(IsWalkingHash, true);
                animator.SetBool(IsRetreatingHash, false);
                break;

            case StalkerState.Retreating:
                agent.isStopped = false;
                agent.speed = retreatSpeed;
                animator.SetBool(IsWalkingHash, false);
                animator.SetBool(IsRetreatingHash, true);
                break;

            case StalkerState.Attacking:
                agent.isStopped = true;
                animator.SetBool(IsWalkingHash, false);
                animator.SetBool(IsRetreatingHash, false);
                animator.SetTrigger(AttackTriggerHash);
                StartCoroutine(HandleAttackSequence());
                break;
        }

        currentState = newState;
    }

    private IEnumerator HandleAttackSequence()
    {
        // Use existing GameManager system for attack sequence
        GameManager.Instance.OnPlayerCaught(() => {
            // Reset position during black screen
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            isAffectedByTorch = false;
            TransitionToState(StalkerState.Pursuing);
        });

        yield return null;
    }
}