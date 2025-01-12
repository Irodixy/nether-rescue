using UnityEngine;
using System.Collections;

public class LockablePassage : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float unlockDuration = 3f;
    [SerializeField] private float interactionRange = 2f;
    
    [Header("References")]
    [SerializeField] private GameObject lockedObject; // The object to disable when unlocked
    
    private bool isUnlocking = false;
    private bool isUnlocked = false;
    private bool screwdriverEquip = false;
    private ObjectInteractionController playerController;
    
    public float InteractionRange => interactionRange;
    
    public string InteractionPrompt
    {
        get
        {
            if (isUnlocked) return string.Empty;
            
            if (!playerController.hasScrewdriver) return "Locked, need something to open";
            
            /*if (playerController.hasScrewdriver) 
                return "Locked, need something to open";*/
            
            if (playerController.hasScrewdriver && !(playerController.currentObject is Screwdriver))
                return "Equip screwdriver to unlock";
                
            return "Press E to unlock";
        }
    }

    private void Start()
    {
        // Find the player's interaction controller once at start
        playerController = GameObject.FindGameObjectWithTag("Player")
            .GetComponent<ObjectInteractionController>();
        lockedObject = gameObject;
    }

    public bool CanInteract(MovimentarJogador player)
    {
        if (isUnlocked || isUnlocking) return false;
        
        // Can only interact if player has screwdriver equipped
        return playerController.currentObject is Screwdriver;
    }

    public void Interact(MovimentarJogador player)
    {
        if (!CanInteract(player) || isUnlocking) return;
        
        StartCoroutine(UnlockPassage());
    }

    private IEnumerator UnlockPassage()
    {
        isUnlocking = true;
        float elapsedTime = 0f;

        // Get the screwdriver component to trigger its animation/effects
        Screwdriver screwdriver = playerController.currentObject as Screwdriver;
        if (screwdriver != null)
        {
            screwdriver.OnUse(playerController.GetComponent<MovimentarJogador>());
        }

        while (elapsedTime < unlockDuration)
        {
            // Check if player moved away or unequipped screwdriver
            if (!(playerController.currentObject is Screwdriver))
            {
                isUnlocking = false;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Unlock the passage
        isUnlocked = true;
        isUnlocking = false;
        
        if (lockedObject != null)
        {
            lockedObject.SetActive(false);
        }
    }
}