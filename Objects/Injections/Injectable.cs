using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum to define different types of injectable effects
public enum InjectableEffect
{
    SpeedBoost,
    JumpBoost
}

[System.Serializable]
public class EffectParameters
{
    public float multiplier = 2f;
    public float duration = 10f;
    public Color glowColor = Color.cyan;
}

public class Injectable : MonoBehaviour, IInteractable
{
    [Header("Base Injectable Settings")]
    [SerializeField] private GameObject objectThis;
    [SerializeField] private InjectableEffect effectType;
    [SerializeField] private EffectParameters effectParams;
    [SerializeField] private float interactionRange = 2f;

    [Header("Visual Effects")]
    [SerializeField] private Light glowLight;
    [SerializeField] private float glowIntensity = 1f;
    [SerializeField] private float pulsateSpeed = 1f;
    [SerializeField] private float minIntensity = 0.5f;

    private Injections injectionController;
    private ObjectInteractionController interactionController;

    // IInteractable implementation
    public string InteractionPrompt => $"Press E to collect {effectType} injectable";
    public float InteractionRange => interactionRange;

    private void Start()
    {
        InitializeGlow();
        // Ensure references
        if (injectionController == null) injectionController = objectThis.GetComponent<Injections>();
        if (interactionController == null) interactionController = GameObject.Find("Jogador").GetComponent<ObjectInteractionController>();
        objectThis = gameObject;
    }

    private void InitializeGlow()
    {
        if (glowLight == null)
        {
            // Create and setup glow light if not assigned
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;

            glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.intensity = glowIntensity;
            glowLight.range = interactionRange;
        }

        glowLight.color = effectParams.glowColor;
    }

    private void Update()
    {
        // Create pulsating effect
        if (glowLight != null)
        {
            float pulse = (Mathf.Sin(Time.time * pulsateSpeed) + 1f) / 2f;
            glowLight.intensity = Mathf.Lerp(minIntensity, glowIntensity, pulse);
        }
    }

    public bool CanInteract(MovimentarJogador player) => true;

    public void Interact(MovimentarJogador player)
    { 
        if (injectionController != null)
        {
            if (interactionController != null)
            {
                interactionController.CollectObject("Injections");
                if (interactionController.currentInjectable != null)
                {
                    injectionController.DropCurrent();
                }
                interactionController.currentInjectable = this;

                //injectionController.CollectInjectable(this, effectType, effectParams);
                injectionController.StartApplySpeedBoost(this);
                // The InjectionController will handle destroying this object
            }
        }
    }

    // Public getter for effect parameters (useful for preview in editor)
    public EffectParameters GetEffectParameters() => effectParams;

    public InjectableEffect GetInjectableEffect() => effectType;
}
