using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screwdriver : InteractableObject
{
    [Header("Screwdriver Interaction")]
    [SerializeField] private float interactionDuration = 2f;

    private float currentInteractionTime = 0f;
    private bool isInteracting = false;

    public override void OnEquip(MovimentarJogador player, float x = 1f)
    {
        // Equip screwdriver
        //player.GetComponentInChildren<Animator>().SetTrigger("HoldScrewdriver");
    }

    public override float OnUnequip(MovimentarJogador player)
    {
        //player.GetComponentInChildren<Animator>().SetTrigger("HoldTorch");
        return 1f;
    }

    public override void OnUse(MovimentarJogador player)
    {
        // Start interaction with vent or other object
        isInteracting = true;
        currentInteractionTime = 0f;
    }

    public override void OnSecondaryUse(MovimentarJogador player)
    {
        // Optional: Could be used for additional interactions
    }

    private void Update()
    {
        if (isInteracting)
        {
            currentInteractionTime += Time.deltaTime;

            if (currentInteractionTime >= interactionDuration)
            {
                CompleteInteraction();
            }
        }
    }

    private void CompleteInteraction()
    {
        // Trigger interaction complete logic
        // For example, open a vent
        Debug.Log("Interaction completed");
        isInteracting = false;
    }
}