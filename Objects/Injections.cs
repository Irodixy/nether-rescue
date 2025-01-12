using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Injections : InteractableObject//MonoBehaviour
{
    [SerializeField] private Transform injectableAttachPoint;
    private Injectable currentInjectable;
    private MovimentarJogador playerMovement;
    private Dictionary<InjectableEffect, Coroutine> activeEffects;

    private ObjectInteractionController interactionController;

    private void Awake()
    {
        playerMovement = GetComponent<MovimentarJogador>();
        activeEffects = new Dictionary<InjectableEffect, Coroutine>();
    }

    private void Start()
    {
        if (interactionController == null) interactionController = GameObject.Find("Jogador").GetComponent<ObjectInteractionController>();
    }

    private void OnEnable()
    {
        ObjectInteractionController.OnActivateInjection += ActivateThis;
    }

    private void OnDisable()
    {
        ObjectInteractionController.OnActivateInjection -= ActivateThis;
    }

    public InteractableObject ActivateThis()
    {
        return this;
    }

    // Abstract methods to enforce implementation in child classes
    public override void OnEquip(MovimentarJogador player, float munition)
    {
        currentInjectable = getTheCurrentInjection();
        currentInjectable.gameObject.SetActive(true);
    }
    public override float OnUnequip(MovimentarJogador player)
    {
        currentInjectable = getTheCurrentInjection();
        currentInjectable.gameObject.SetActive(false);
        return currentCharge = 1f;
    }
    public override void OnUse(MovimentarJogador player)
    {
        UseInjectable();
    }
    public override void OnSecondaryUse(MovimentarJogador player)
    {

    }
    private Injectable getTheCurrentInjection()
    {
        return interactionController.currentInjectable;
    }
    public void CollectInjectable(Injectable newInjectable, InjectableEffect effectType, EffectParameters parameters)
    {
        if (interactionController.currentInjectable = newInjectable)
        {
            currentInjectable = getTheCurrentInjection();
            // Store new injectable
            currentInjectable = newInjectable;
            Debug.Log(currentInjectable);
            currentInjectable.transform.SetParent(injectableAttachPoint);
            currentInjectable.transform.localPosition = Vector3.zero;
            currentInjectable.transform.localRotation = Quaternion.identity;
            // Is always there, but the player only sees when he choose to
            currentInjectable.gameObject.SetActive(false);
            Debug.Log(currentInjectable);
        }
    }

    public void DropCurrent()
    {
        currentInjectable = getTheCurrentInjection();
        if (currentInjectable != null)
        {
            currentInjectable.gameObject.SetActive(true);
            currentInjectable.transform.SetParent(null);
            // Add some random force to throw it away
            var rb = currentInjectable.gameObject.AddComponent<Rigidbody>();
            rb.AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            //Destroy(rb, 1f); // Remove rigidbody after a second
        }
    }

    public void UseInjectable()
    {
        currentInjectable = getTheCurrentInjection();
        //var injectableComponent = currentInjectable.GetComponent<Injectable>();
        var effectType = /*injectableComponent*/currentInjectable.GetInjectableEffect();
        var parameters = /*injectableComponent*/currentInjectable.GetEffectParameters();
        Debug.Log(effectType);
        Debug.Log(parameters);
        //var effectType = currentInjectable.GetComponent<Injectable>().GetEffectParameters();
        ApplyEffect(effectType, parameters);
        Destroy(currentInjectable.gameObject);
        Debug.Log("como raio cheguei aqui?");
        currentInjectable = null;
    }

    private void ApplyEffect(InjectableEffect effectType, EffectParameters parameters)
    {
        switch (effectType)
        {
            case InjectableEffect.SpeedBoost:
                StartEffect(effectType, ApplySpeedBoost());
                break;
            case InjectableEffect.JumpBoost:
                StartEffect(effectType, ApplyJumpBoost());
                break;
        }
    }

    private void StartEffect(InjectableEffect effectType, IEnumerator effectCoroutine)
    {
        // Cancel existing effect of same type if active
        if (activeEffects.TryGetValue(effectType, out Coroutine existing))
        {
            StopCoroutine(existing);
        }

        // Start new effect
        activeEffects[effectType] = StartCoroutine(effectCoroutine);
    }

    private IEnumerator ApplySpeedBoost()
    {
        playerMovement.speedMultiplier *= 2f;
        yield return new WaitForSeconds(10f);
        playerMovement.speedMultiplier /= 2f;
    }

    public IEnumerator StartApplySpeedBoost(Injectable newInjectable)
    {
        currentInjectable = newInjectable;
        Debug.Log(currentInjectable);
        currentInjectable.transform.SetParent(injectableAttachPoint);
        currentInjectable.transform.localPosition = Vector3.zero;
        currentInjectable.transform.localRotation = Quaternion.identity;
        playerMovement.speedMultiplier *= 2f;
        yield return new WaitForSeconds(10f);
        playerMovement.speedMultiplier /= 2f;
        //StartCoroutine(TestApplySpeedBoost());
    }
    private IEnumerator TestApplySpeedBoost()
    {
        playerMovement.speedMultiplier *= 2f;
        yield return new WaitForSeconds(10f);
        playerMovement.speedMultiplier /= 2f;
    }

    private IEnumerator ApplyJumpBoost()
    {
        playerMovement.jumpMultiplier *= 2f;
        yield return new WaitForSeconds(10f);
        playerMovement.jumpMultiplier /= 2f;
    }
}