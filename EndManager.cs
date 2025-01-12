using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

// Enum to define types of tutorial actions
public enum EndActionType
{
    Dialogue,
    Interaction,
}

// Class to define a single tutorial step
[System.Serializable]
public class EndStep
{
    public string stepId;
    public EndActionType actionType;
    public GameObject targetObject;
    public DialogueNode[] dialogueNodes;
    public bool requiresInput;
    public string description; // For debugging and editor clarity
}

public class EndManager : MonoBehaviour
{
    [Header("End Configuration")]
    [SerializeField] private List<EndStep> endSteps = new List<EndStep>();
    [SerializeField] private bool startEnd = false;

    [Header("Input Settings")]
    [SerializeField] private KeyCode progressKey = KeyCode.E;

    private EndStep currentStep;
    private int currentStepIndex = -1;
    private DialogueManager dialogueManager;
    private bool isStepComplete = false;
    private bool isEndActive = false;
    private bool objectInteracted = false;

    public static event Action<string> OnEndStepCompleted;
    public static event Action OnEndCompleted;

    private GameObject firstPart;
    private GameObject secondPart;

    private void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        /*firstPart = GameObject.Find("FirstPart");
        firstPart.SetActive(true);
        secondPart = GameObject.Find("SecondPart");
        secondPart.SetActive(false);*/

        if (startEnd)
        {
            StartEnd();
        }
    }

    private void OnEnable()
    {
        // Subscribe to dialogue completion event
        DialogueManager.OnDialogueSequenceComplete += HandleDialogueComplete;
        // Subscribe for handling the object interactions
        InteractForEnd.OnInteracted += HandleObjectInteracted;
        BeginEnd.OnInteracted += StartIt;
        BeginEnd.OnSpeedRun += CompleteTutorial;
    }

    private void OnDisable()
    {
        // Unsubscribe to dialogue completion event
        DialogueManager.OnDialogueSequenceComplete -= HandleDialogueComplete;
        // Unsubscribe for handling the object interactions
        InteractForEnd.OnInteracted -= HandleObjectInteracted;
        BeginEnd.OnInteracted -= StartIt;
        BeginEnd.OnSpeedRun -= CompleteTutorial;
    }

    private void StartIt()
    {
        startEnd = true;
        StartEnd();
    }

    public void StartEnd()
    {
        isEndActive = true;
        currentStepIndex = -1;
        ProgressToNextStep();
    }

    private void Update()
    {
        if (!isEndActive || currentStepIndex >= endSteps.Count) return;

        switch (currentStep.actionType)
        {
            case EndActionType.Dialogue:
                if (currentStep.requiresInput && Input.GetKeyDown(progressKey))
                {
                    Destroy(currentStep.targetObject);
                    CompleteCurrentStep();
                }
                break;

            case EndActionType.Interaction:
                if (objectInteracted)
                {
                    // reset script activation
                    var interactable = currentStep.targetObject.GetComponent<InteractForTutorial>();
                    Destroy(interactable);
                    objectInteracted = false;
                    CompleteCurrentStep();
                }
                break;
        }
    }

    private void ProgressToNextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= endSteps.Count)
        {
            CompleteTutorial();
            return;
        }

        currentStep = endSteps[currentStepIndex];
        StartStep(currentStep);
    }

    private void StartStep(EndStep step)
    {
        isStepComplete = false;

        // Enable the target object if it exists
        if (step.targetObject != null)
        {
            step.targetObject.SetActive(true);
        }

        switch (step.actionType)
        {
            case EndActionType.Dialogue:
                StartDialogueStep(step);
                break;

            case EndActionType.Interaction:
                StartDialogueStep(step); //reset the subtitles
                StartInteractionStep(step);
                break;
        }
    }

    private void StartDialogueStep(EndStep step)
    {
        if (dialogueManager != null && step.dialogueNodes != null)
        {
            dialogueManager.dialogueNodes = step.dialogueNodes;
            dialogueManager.StartDialogue(step.targetObject, true);
        }
    }

    private void StartInteractionStep(EndStep step)
    {
        if (step.targetObject != null)
        {
            step.targetObject.AddComponent<InteractForEnd>();
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

    private void HandleDialogueComplete()
    {
        if (currentStepIndex >= 0 && currentStepIndex < endSteps.Count)
        {
            //TutorialStep currentStep = tutorialSteps[currentStepIndex];
            if (currentStep.actionType == EndActionType.Dialogue)
            {
                CompleteCurrentStep();
            }
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
        isEndActive = false;
        //OnEndCompleted?.Invoke();
        secondPart.SetActive(true);
        firstPart.SetActive(false);
        Debug.Log("End completed!");
    }
}