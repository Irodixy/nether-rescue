using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupForTutorial : MonoBehaviour, IInteractable
{
    public static event Action<GameObject> OnInteracted;

    //[SerializeField] private GameObject thisObject;
    [SerializeField] private float interactionRange = 2f; // Range within which the player can interact
    //private bool interacted = false;

    public string InteractionPrompt => "Press E to pickup";  // UI Prompt
    public float InteractionRange => interactionRange;

    public bool CanInteract(MovimentarJogador player) => true;

   public void Interact(MovimentarJogador player)
   {
        // Notify listeners that the object was picked up
        OnInteracted?.Invoke(this.gameObject);

        // Remove pickup item from world
        Destroy(this.gameObject);
    }
}
