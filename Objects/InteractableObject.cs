using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectInteractionType
{
    Screwdriver,        // Screwdriver, general tools
    Torch,              // Torch as weapon, rocks
    Rock,               // Placeholder for future injections
    Injections          // Objects used for specific interactions
}

public abstract class InteractableObject : MonoBehaviour
{
    [Header("Object Interaction Properties")]
    [SerializeField] protected string objectName = "Generic Object";
    [SerializeField] public ObjectInteractionType interactionType;

    [Header("Animation References")]
    [SerializeField] public string equipAnimationTrigger = "Equip";
    [SerializeField] protected string useAnimationTrigger = "Use";

    [Header("Object Mechanics")]
    [SerializeField] protected bool isReusable = true;
    [SerializeField] protected float objectDurability = 100f;

    public float currentCharge;


    // Abstract methods to enforce implementation in child classes
    public abstract void OnEquip(MovimentarJogador player, float munition);
    public abstract float OnUnequip(MovimentarJogador player);
    public abstract void OnUse(MovimentarJogador player);
    public abstract void OnSecondaryUse(MovimentarJogador player);

    public virtual bool CanInteract() => true;
}