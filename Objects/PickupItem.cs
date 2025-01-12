using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private string itemName = "Item";
    [SerializeField] private float interactionRange = 2f;

    public string InteractionPrompt => $"Press E to pick up {itemName}";
    public float InteractionRange => interactionRange;

    private ObjectInteractionController interactionController;

    public bool CanInteract(MovimentarJogador player) => true;

    private void Start()
    {
        // Ensure references
        if (interactionController == null) interactionController = GameObject.Find("Jogador").GetComponent<ObjectInteractionController>();
    }

    public void Interact(MovimentarJogador player)
    { 
        // Find the ObjectInteractionController on the player

        
        //ObjectInteractionController interactionController = GameObject.Find("Jogador").GetComponent<ObjectInteractionController>();

        // interactionController.hasTorch = false;

        if (interactionController != null && objectPrefab != null)
        {
            if (checkCharge())
            {
                Debug.Log("Already full!!");
                return;
            }
            Debug.Log($"Adding {objectPrefab.GetType().Name} to persistent inventory");
            Debug.Log(objectPrefab);
            // Ensure the object is added to the player's persistent inventory
            interactionController.CollectObject(itemName);
            //Debug.Log("has torch" + interactionController.hasTorch);
            // Remove pickup item from world
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Interaction failed - ObjectInteractionController or objectPrefab is missing.");
        }
    }

    private bool checkCharge()
    {
        if(interactionController.currentFuel == 100f || interactionController.numberRocks == 5f)
        {
            return true;
        }
        return false;
    }
}
