using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StairInteractable : MonoBehaviour, IInteractable
{
    public static event Action<GameObject> OnInteracted;

    [SerializeField] private Transform topPosition;  // Reference to the top position
    [SerializeField] private Transform bottomPosition;  // Reference to the bottom position
    [SerializeField] private float interactionRange = 2f;  // Range within which the player can interact
    [SerializeField] private TutorialManager tutorialManager = null;

    public string InteractionPrompt => "Press E to use the stairs";  // UI Prompt
    public float InteractionRange => interactionRange;

    private void Start()
    {
        var parentTransform = transform.parent;

        if (parentTransform != null)
        {
            GameObject parentGameObject = parentTransform.gameObject;

            if (topPosition == null)
            {
                // Use Transform.Find to locate the child object by name
                Transform topTransform = parentTransform.Find("Top");
                if (topTransform != null)
                {
                    topPosition = topTransform; // Assign the Transform of "Top"
                }
                else
                {
                    Debug.LogWarning("Top object not found as a child of the parent!");
                }
            }

            if (bottomPosition == null)
            {
                // Use Transform.Find to locate the child object by name
                Transform bottomTransform = parentTransform.Find("Bottom");
                if (bottomTransform != null)
                {
                    bottomPosition = bottomTransform; // Assign the Transform of "Top"
                }
                else
                {
                    Debug.LogWarning("Bottom object not found as a child of the parent!");
                }
            }
        }
        else
        {
            Debug.LogWarning("This object has no parent!");
        }

    }

    public bool CanInteract(MovimentarJogador player) => true;

    public void Interact(MovimentarJogador player)
    {
        var currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "TutorialScene")
        {
            if (GameObject.Find("TutorialGameManager").TryGetComponent<TutorialManager>(out tutorialManager))
            {
                // Notify listeners that the ladder was used
                OnInteracted?.Invoke(this.gameObject);
            }
        }
        

        GameObject playerInScene = GameObject.FindGameObjectWithTag("Player");

        // Calculate distances
        float distanceToTop = Vector3.Distance(playerInScene.transform.position, topPosition.position);
        float distanceToBottom = Vector3.Distance(playerInScene.transform.position, bottomPosition.position);
        Debug.Log(distanceToTop);
        Debug.Log(distanceToBottom);
        // Define a small threshold to avoid teleporting back to the same position
        float threshold = 0.5f;

        if (distanceToTop > distanceToBottom - threshold)
        {
            TeleportPlayer(playerInScene, topPosition.position);
        }
        else if (distanceToBottom > distanceToTop - threshold)
        {
            TeleportPlayer(playerInScene, bottomPosition.position);
        }
        else
        {
            Debug.LogWarning("Player is already at the closest position. No teleportation performed.");
        }
    }

    private void TeleportPlayer(GameObject playerInScene, Vector3 targetPosition)
    {
        CharacterController controller = playerInScene.GetComponent<CharacterController>();

        if (controller != null)
        {
            // Disable CharacterController temporarily
            controller.enabled = false;
        }
        // Set position directly
        playerInScene.transform.position = targetPosition;
        Debug.Log("Player teleported to: " + targetPosition);

        if (controller != null)
        {
            // Re-enable CharacterController
            controller.enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (topPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(topPosition.position, 0.2f);
        }

        if (bottomPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(bottomPosition.position, 0.2f);
        }
    }
}
