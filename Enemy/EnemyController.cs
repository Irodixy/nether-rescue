using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerTransform;

    [Header("Range Settings")]
    [SerializeField] private float detectionRange = 10f;        // Distance to maintain chase
    [SerializeField] private float activationRange = 7f;        // Distance to start chasing
    [SerializeField] private float attackRange = 1.5f;          // Distance to trigger attack

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float returnSpeed = 2f;            // Speed when returning to original position
    [SerializeField] private float returnThreshold = 0.5f;      // How close to get to original position

    [Header("Audio Settings")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // Animation parameter hashes for better performance
    private readonly int IsIdleHash = Animator.StringToHash("IsIdle");
    private readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    // Component references
    private NavMeshAgent agent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isActivated = false;

    // State management
    private enum EnemyState { Idle, Chase, Attack, Returning }
    private EnemyState currentState;

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

        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Configure NavMeshAgent
        agent.speed = patrolSpeed;
        agent.stoppingDistance = attackRange;
        agent.acceleration = 12f; // Quick acceleration for responsive movement

        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private void SaveOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void SetInitialState()
    {
        TransitionToState(EnemyState.Idle);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;

            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                break;

            case EnemyState.Attack:
                HandleAttackState();
                break;

            case EnemyState.Returning:
                HandleReturningState(distanceToPlayer);
                break;
        }
    }

    private void HandleIdleState(float distanceToPlayer)
    {
        // Check for initial activation or if already activated
        if (!isActivated && distanceToPlayer <= activationRange)
        {
            isActivated = true;
            TransitionToState(EnemyState.Chase);
        }
        else if (isActivated && distanceToPlayer <= detectionRange)
        {
            TransitionToState(EnemyState.Chase);
        }
    }

    private void HandleChaseState(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange)
        {
            TransitionToState(EnemyState.Idle);
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            TransitionToState(EnemyState.Attack);
            return;
        }

        // Update destination while chasing
        agent.SetDestination(playerTransform.position);
    }

    private void HandleAttackState()
    {
        // Trigger attack animation and effects
        animator.SetTrigger(AttackTriggerHash);

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        StartCoroutine(HandleAttackSequence());
    }

    private void HandleReturningState(float distanceToPlayer)
    {
        // Check if player comes back in range while returning
        if (isActivated && distanceToPlayer <= detectionRange)
        {
            TransitionToState(EnemyState.Chase);
            return;
        }
        
        float distanceToOriginal = Vector3.Distance(transform.position, originalPosition);
        
        if (distanceToOriginal <= returnThreshold)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            TransitionToState(EnemyState.Idle);
            return;
        }
        
        agent.SetDestination(originalPosition);
    }

    private IEnumerator HandleAttackSequence()
    {
        // Tell GameManager to start the black screen transition
        GameManager.Instance.OnPlayerCaught(
            // Callback for when screen is black
            () => {
                // Reset positions during black screen
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                TransitionToState(EnemyState.Idle);
            }
        );

        yield return null;
    }

    private void TransitionToState(EnemyState newState)
    {
        // Exit current state
        switch (currentState)
        {
            case EnemyState.Chase:
                agent.speed = patrolSpeed;
                break;
        }

        // Enter new state
        switch (newState)
        {
            case EnemyState.Idle:
                agent.isStopped = true;
                animator.SetBool(IsIdleHash, true);
                animator.SetBool(IsWalkingHash, false);
                animator.SetBool(IsRunningHash, false);
                break;

            case EnemyState.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                animator.SetBool(IsIdleHash, false);
                animator.SetBool(IsWalkingHash, false);
                animator.SetBool(IsRunningHash, true);
                break;

            case EnemyState.Attack:
                agent.isStopped = true;
                animator.SetBool(IsWalkingHash, false);
                animator.SetBool(IsRunningHash, false);
                break;

            case EnemyState.Returning:
                agent.isStopped = false;
                agent.speed = returnSpeed;
                animator.SetBool(IsIdleHash, false);
                animator.SetBool(IsRunningHash, false);
                animator.SetBool(IsWalkingHash, true);
                break;
        }

        currentState = newState;
    }
}