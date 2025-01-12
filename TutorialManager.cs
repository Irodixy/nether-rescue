using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

// Enum to define types of tutorial actions
public enum TutorialActionType
{
    Dialogue,
    Interaction,
    PickupItem,
    UseStair
}

// Class to define a single tutorial step
[System.Serializable]
public class TutorialStep
{
    public string stepId;
    public TutorialActionType actionType;
    public GameObject targetObject;
    public DialogueNode[] dialogueNodes;
    public bool requiresInput;
    public string description; // For debugging and editor clarity
}

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Configuration")]
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    [SerializeField] private bool startTutorialOnAwake = true;

    [Header("Input Settings")]
    [SerializeField] private KeyCode progressKey = KeyCode.E;

    private PickupForTutorial scriptPickupTutorial;
    private TutorialStep currentStep;
    private int currentStepIndex = -1;
    private DialogueManager dialogueManager;
    private bool isStepComplete = false;
    private bool isTutorialActive = false;
    private bool objectPickedUp = false;
    private bool objectInteracted = false;
    private bool stairUsed = false;

    public static event Action<string> OnTutorialStepCompleted;
    public static event Action OnTutorialCompleted;

    private void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (startTutorialOnAwake)
        {
            StartTutorial();
        }
    }

    private void OnEnable()
    {
        // Subscribe to dialogue completion event
        DialogueManager.OnDialogueSequenceComplete += HandleDialogueComplete;
        // Subscribe for handling the object interactions
        PickupForTutorial.OnInteracted += HandleObjectPickedUp;
        InteractForTutorial.OnInteracted += HandleObjectInteracted;
        StairInteractable.OnInteracted += HandleUseStair;
    }

    private void OnDisable()
    {
        // Unsubscribe to dialogue completion event
        DialogueManager.OnDialogueSequenceComplete -= HandleDialogueComplete;
        // Unsubscribe for handling the object interactions
        PickupForTutorial.OnInteracted -= HandleObjectPickedUp;
        InteractForTutorial.OnInteracted -= HandleObjectInteracted;
        StairInteractable.OnInteracted -= HandleUseStair;
    }

    public void StartTutorial()
    {
        isTutorialActive = true;
        currentStepIndex = -1;
        ProgressToNextStep();
    }

    private void Update()
    {
        if (!isTutorialActive || currentStepIndex >= tutorialSteps.Count) return;

        switch (currentStep.actionType)
        {
            case TutorialActionType.Dialogue:
                if (currentStep.requiresInput && Input.GetKeyDown(progressKey))
                {
                    Destroy(currentStep.targetObject);
                    CompleteCurrentStep();
                }
                break;

            case TutorialActionType.Interaction:
                if (objectInteracted)
                {
                    // reset script activation
                    var interactable = currentStep.targetObject.GetComponent<InteractForTutorial>();
                    Destroy(interactable);
                    objectInteracted = false;
                    CompleteCurrentStep();
                }
                break;

            case TutorialActionType.PickupItem:
                if (objectPickedUp)
                {
                    objectPickedUp = false;
                    CompleteCurrentStep();
                }
                break;

            case TutorialActionType.UseStair:
                if (stairUsed)
                {
                    // reset script activation
                    var interactable = currentStep.targetObject.GetComponent<StairInteractable>();
                    Destroy(interactable);
                    stairUsed = false;
                    CompleteCurrentStep();
                }
                break;
        }
    }

    private void ProgressToNextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= tutorialSteps.Count)
        {
            CompleteTutorial();
            return;
        }

        currentStep = tutorialSteps[currentStepIndex];
        StartStep(currentStep);
    }

    private void StartStep(TutorialStep step)
    {
        isStepComplete = false;

        // Enable the target object if it exists
        if (step.targetObject != null)
        {
            step.targetObject.SetActive(true);
        }

        switch (step.actionType)
        {
            case TutorialActionType.Dialogue:
                StartDialogueStep(step);
                break;

            case TutorialActionType.Interaction:
                StartDialogueStep(step); //reset the subtitles
                StartInteractionStep(step);
                break;

            case TutorialActionType.PickupItem:
                StartDialogueStep(step); //reset the subtitles
                StartPickupItemStep(step);
                break;

            case TutorialActionType.UseStair:
                StartDialogueStep(step); //reset the subtitles
                StartUseStairStep(step);
                break;
        }
    }

    private void StartDialogueStep(TutorialStep step)
    {
        if (dialogueManager != null && step.dialogueNodes != null)
        {
            dialogueManager.dialogueNodes = step.dialogueNodes;
            dialogueManager.StartDialogue(step.targetObject, true);
        }
    }

    private void StartInteractionStep(TutorialStep step)
    {
        if (step.targetObject != null)
        {
            step.targetObject.AddComponent<InteractForTutorial>();
        }
    }

    private void StartPickupItemStep(TutorialStep step)
    {
        if (step.targetObject != null)
        {
            step.targetObject.AddComponent<PickupForTutorial>();
        }
    }

    private void StartUseStairStep(TutorialStep step)
    {
        if (step.targetObject != null)
        {
            step.targetObject.AddComponent<StairInteractable>();
        }
    }

    private void HandleObjectInteracted(GameObject interactedObject)
    {
        // Check if the picked-up object is the one we are monitoring
        if (interactedObject == currentStep.targetObject)
        {
            objectInteracted = true;
        }
    }

    private void HandleObjectPickedUp(GameObject pickedUpObject)
    {
        // Check if the picked-up object is the one we are monitoring
        if (pickedUpObject == currentStep.targetObject)
        {
            objectPickedUp = true;
        }
    }

    private void HandleDialogueComplete()
    {
        if (currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Count)
        {
            //TutorialStep currentStep = tutorialSteps[currentStepIndex];
            if (currentStep.actionType == TutorialActionType.Dialogue)
            {
                CompleteCurrentStep();
            }
        }
    }

    private void HandleUseStair(GameObject usedStair)
    {
        // Check if the picked-up object is the one we are monitoring
        if (usedStair == currentStep.targetObject)
        {
            stairUsed = true;
        }
    }

    private void CompleteCurrentStep()
    {
        if (isStepComplete) return;

        isStepComplete = true;

        // Notify listeners of step completion
        //OnTutorialStepCompleted?.Invoke(currentStep.stepId);

        // Progress to next step
        ProgressToNextStep();
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        OnTutorialCompleted?.Invoke();
        Debug.Log("Tutorial completed!");
    }
}