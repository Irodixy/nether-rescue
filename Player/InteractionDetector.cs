using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovimentarJogador))]
public class InteractionDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask obstacleLayer; // Layer for objects that should block interaction
    [SerializeField] private LayerMask interactableLayer; // Layer for interactable objects
    [SerializeField] private float viewAngle = 180f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private MovimentarJogador player;
    private IInteractable currentInteractable;

    private void Start()
    {
        player = GetComponent<MovimentarJogador>();
    }

    private void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;

        DetectInteractables();
        HandleInteraction();
    }

    private void DetectInteractables()
    {
        if (InteractionUI.Instance == null)
        {
            Debug.LogError("InteractionUI instance is missing in the scene!");
            return;
        }

        // Find all potential interactables in range
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);
        
        float closestDistance = float.MaxValue;
        IInteractable closestInteractable = null;

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<IInteractable>(out var interactable))
            {
                // Check if within interaction range
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance > interactable.InteractionRange)
                {
                    continue;
                }
                
                // Check if player is facing the interactable
                Vector3 directionToInteractable = (collider.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToInteractable);

                if (angle < viewAngle)
                {
                    // Check only for obstacles, excluding the interactable layer
                    if (!Physics.Raycast(transform.position, directionToInteractable, out RaycastHit hit, distance, obstacleLayer))
                    {
                        Debug.DrawLine(transform.position, collider.transform.position, Color.green, 0.1f);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestInteractable = interactable;
                        }
                    }
                    else
                    {
                        Debug.DrawLine(transform.position, hit.point, Color.red, 0.1f);
                        Debug.Log($"Obstacle detected: {hit.collider.name}");
                    }
                }
            }
        }

        // Update current interactable
        if (closestInteractable != null && closestInteractable.CanInteract(player))
        {
            currentInteractable = closestInteractable;
            InteractionUI.Instance.ShowPrompt(currentInteractable.InteractionPrompt);
        }
        else
        {
            currentInteractable = null;
            InteractionUI.Instance.HidePrompt();
        }
    }

    private void HandleInteraction()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact(player);
        }
    }

    // Optional: Debug visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw view angle
        Vector3 forward = transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -viewAngle, 0) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, right * detectionRadius);
        Gizmos.DrawRay(transform.position, left * detectionRadius);
    }
}