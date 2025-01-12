using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInteractionController : MonoBehaviour
{
    public static event Func<InteractableObject> OnActivateInjection;

    [Header("Attachment Points")]
    [SerializeField] private Transform rightHandAttachPoint;
    [SerializeField] private Transform leftHandAttachPoint;

    [Header("Player References")]
    [SerializeField] private MovimentarJogador player;
    [SerializeField] private Animator playerAnimator;

    [Header("Prefabs for Collectible Objects")]
    [SerializeField] private GameObject torchPrefab;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject screwdriverPrefab;
    public Injectable currentInjectable;

    [Header("Collection Status")]
    public bool hasTorch = false;
    public float currentFuel;
    public bool hasRock = false;
    public float numberRocks;
    public bool hasScrewdriver = false;
    public bool hasInjection = false;

    // Current object in hand
    public InteractableObject currentObject;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Ensure references
        if (player == null) player = GetComponent<MovimentarJogador>();
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Deactivate current object when Q is pressed
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentObject is Screwdriver screwdriver && hasScrewdriver)
            {
                Destroy(currentObject.gameObject);
                currentObject = null;
            }
            else if (hasScrewdriver)
            {
                ActivateScrewdriver();
                currentObject.OnEquip(player, 0f);
            }
            else
            {
                return;
            }
        }

        // Activate Torch with F key
        if (Input.GetKeyDown(KeyCode.F))
        {
            if(currentObject is Torch torch && hasTorch)
            {
                Destroy(currentObject.gameObject);
                currentFuel = currentObject.OnUnequip(player);
                if(currentFuel <= 0) currentFuel = 0;
                currentObject = null;
            }
            else if (hasTorch)
            {
                ActivateTorch();
                currentObject.OnEquip(player, currentFuel);
            }
            else
            {
                return;
            }
        }

        // Handle rocks
        if(Input.GetMouseButtonDown(1) && hasRock)
        {
            // if rock already activated and is in hand
            /*if (currentObject is Rock rock)
            {
                // if press to shoot
                if (Input.GetMouseButtonDown(0))
                {
                    currentObject.OnSecondaryUse(player);
                }
                else
                {
                    currentObject.OnUse(player);
                }
            }
            // if rock is not activated, but there is rocks on the Player
            else
            {
                // equip rock
                ActiveRock();
                currentObject.OnEquip(player, numberRocks);
            }*/
            if (currentObject is not Rock rock)
            {
                ActiveRock();
                currentObject.OnEquip(player, numberRocks);
                HandleObjectInteractions();
            }
        }

        // Ready Injection
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (currentObject is not Injections injections && hasInjection)
            {
                ActivateInjection();
                currentObject.OnEquip(player, 1f);
                HandleObjectInteractions();
            }
            else if (currentObject is Injections)
            {
                currentObject.OnUnequip(player);
                currentObject = null;

            }
        }

        // Handle object interactions if an object is active
        if (currentObject != null)
        {
            HandleObjectInteractions();
        }
    }

    // Activate the torch if collected
    private void ActivateTorch()
    {
        if (hasTorch)
        {
            if (currentObject != null) Destroy(currentObject.gameObject);

            currentObject = Instantiate(torchPrefab, rightHandAttachPoint.position, rightHandAttachPoint.rotation, rightHandAttachPoint).GetComponent<InteractableObject>();
            playerAnimator.SetTrigger(currentObject.equipAnimationTrigger);
            Debug.Log("Torch activated.");
        }
        else
        {
            Debug.Log("Torch not collected yet.");
        }
    }

    private void ActiveRock()
    {
        if (hasRock)
        {
            if (currentObject != null) Destroy(currentObject.gameObject);

            currentObject = Instantiate(rockPrefab, rightHandAttachPoint.position, rightHandAttachPoint.rotation, rightHandAttachPoint).GetComponent<InteractableObject>();
            playerAnimator.SetTrigger(currentObject.equipAnimationTrigger);
            Debug.Log("Rock activated.");
        }
        else
        {
            Debug.Log("Rock not collected yet.");
        }
    }

    private void ActivateScrewdriver()
    {
        if (hasScrewdriver)
        {
            if (currentObject != null) Destroy(currentObject.gameObject);

            currentObject = Instantiate(screwdriverPrefab, rightHandAttachPoint.position, rightHandAttachPoint.rotation, rightHandAttachPoint).GetComponent<InteractableObject>();
            playerAnimator.SetTrigger(currentObject.equipAnimationTrigger);
            Debug.Log("Screwdriver activated.");
        }
        else
        {
            Debug.Log("Screwdriver not collected yet.");
        }
    }

    public void ActivateInjection()
    {
        if (hasInjection)
        {
            if (currentObject != null && currentObject is not Injections) Destroy(currentObject.gameObject);
            currentObject = OnActivateInjection?.Invoke();
            Debug.Log(currentObject);
        }
        
    }

    // Collect an object by its type
    public void CollectObject(string obj)
    {
        if (obj == "Torch")
        {
            // if Player never picked up a torch
            if(!hasTorch) 
            {
                // now he has a torch with full fuel
                hasTorch = true;
                currentFuel = 100f;
            }
            else
            {
                // if current active object is Torch and you picked another
                if(currentObject is Torch torch)
                {
                    // now Player as more 50% fuel to his Torch
                    currentObject.currentCharge += 50f;
                    // doesn't allow fuel to go higher then 100f
                    if (currentObject.currentCharge > 100) currentObject.currentCharge = 100;
                }
                else
                {
                    // same thing has above, but if the Player as not the Torch as the active object, or has no active object
                    currentFuel += 50f;
                    if (currentFuel > 100) currentFuel = 100;
                }
            }
            Debug.Log("Torch collected!");
            Debug.Log(hasTorch);
        }
        else if (obj == "Rock")
        {
            // if Player doesn't have any rocks with him
            if (!hasRock)
            {
                // now he has a rock
                hasRock = true;
                numberRocks = 1f;
            }
            else
            {
                // if current active object is Rock and you picked another
                if (currentObject is Rock rock)
                {
                    // now Player as more 1 rock
                    currentObject.currentCharge += 1f;
                    // doesn't allow rocks to go higher then 5
                    if (currentObject.currentCharge > 5) currentObject.currentCharge = 5;
                }
                else
                {
                    // same thing has above, but if the Player as not the Rock as the active object, or has no active object
                    numberRocks += 1f;
                    if (numberRocks > 5) numberRocks = 5;
                }
            }
            hasRock = true;
            Debug.Log("Rock collected!");
        }
        // if Player didn't picked up the screwdriver
        else if (obj == "Screwdriver")
        {
            hasScrewdriver = true;
            Debug.Log("Screwdriver collected!");
        }
        else if (obj == "Injections")
        {
            hasInjection = true;
            Debug.Log("Injection collected!");
        }
    }

    // Handle object interactions based on current object type
    private void HandleObjectInteractions()
    {
        switch (currentObject.interactionType)
        {
            case ObjectInteractionType.Torch:
                HandleTorchInteraction();
                break;
            case ObjectInteractionType.Rock:
                HandleRockInteraction();
                break;
            case ObjectInteractionType.Screwdriver:
                HandleScrewdriverInteraction();
                break;
            case ObjectInteractionType.Injections:
                HandleInjectionsInteraction();
                break;
        }
    }

    // Handle interactions for tools (e.g., screwdriver)
    private void HandleTorchInteraction()
    {
        // Consume charge when using the object
        if (currentObject is Torch torch)
        {
            Debug.Log(currentObject.currentCharge);
            // Check if there's still charge
            if (currentObject.currentCharge > 0 && Input.GetMouseButton(0))
            {
                // Aiming torch
                currentObject.OnSecondaryUse(player);
            }
            else if (currentObject.currentCharge > 0)
            {
                // Default hold torch
                currentObject.OnUse(player);
            }
            else
            {
                // Torch has no fuel
                Debug.Log("Torch is out of charge!");
            }
        }
    }

    // Handle interactions for weapons (torch, rocks)
    private void HandleRockInteraction()
    {
        if (currentObject is Rock rock)
        {
            // Aim with right mouse button
            if (Input.GetMouseButtonDown(1)) // Right mouse down
            {
                currentObject.OnUse(player);
            }

            // Throw rock when left mouse is clicked during aim
            if (Input.GetMouseButtonDown(0) && Input.GetMouseButton(1)) // Left mouse while right mouse held
            {
                rock.ThrowRock(player);
            }

            // Cancel aim with right mouse up
            if (Input.GetMouseButtonUp(1))
            {
                currentObject.OnSecondaryUse(player);
            }
        }
    }

    private void HandleScrewdriverInteraction()
    {

    }

    private void HandleInjectionsInteraction()
    {
        if (currentObject is Injections injections)
        {
            if(Input.GetMouseButtonDown(0))
            {
                currentObject.OnUse(player);
            }
        }
    }

    // Check for long press duration
    private IEnumerator CheckLongPress()
    {
        float pressDuration = 0f;
        while (Input.GetKey(KeyCode.E))
        {
            pressDuration += Time.deltaTime;
            if (pressDuration >= 1f)
            {
                currentObject.OnUse(player);
                yield break;
            }
            yield return null;
        }
    }

    /*public int GetPersistentObjectsCount()
    {
        return persistentObjects.Count;
    }*/
}