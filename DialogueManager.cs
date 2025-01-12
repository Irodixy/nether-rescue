using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public DialogueNode[] dialogueNodes;
    private int currentNodeIndex = 0;
    private bool isDialogueActive = false;
    private float delayBeforeNextText = 1f;
    private bool toDestroy = false;
    private GameObject zone;

    [SerializeField] private TextMeshProUGUI textDisplay;

    //if comes from tutorial the request
    private bool requestTutorial;

    private void Start()
    {
        //textDisplay = gameObject.GetComponent<TextMeshProUGUI>();
        if (textDisplay == null)
        {
            textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        }
        //textDisplay = FindObjectOfType<TextMeshProUGUI>();
    }

    public void StartDialogue(GameObject zoneActivated, bool fromTutoriual = false)
    {
        requestTutorial = fromTutoriual;
        zone = zoneActivated;
        currentNodeIndex = 0;
        isDialogueActive = true;
        DisplayNextDialogueNode();
    }

    private void DisplayNextDialogueNode()
    {
        if (currentNodeIndex < dialogueNodes.Length)
        {
            DialogueNode node = dialogueNodes[currentNodeIndex];
            StartCoroutine(DisplayDialogueNode(node));
            if (node.playOnce) toDestroy = true;
            currentNodeIndex++;
        }
        else
        {
            isDialogueActive = false;
            //zone = null;
            // Check if is only to play once
            if (toDestroy)
            {
                Destroy(zone);
            }
        }
    }

    private IEnumerator DisplayDialogueNode(DialogueNode node)
    {
        textDisplay.text = node.text;
        // Only for tutorial!

        if (requestTutorial)
        {
            yield return new WaitForSeconds(300000f);
        }
        else
        {
            yield return new WaitForSeconds(node.displayDuration);
        }
        textDisplay.text = "";
        yield return new WaitForSeconds(delayBeforeNextText);
        DisplayNextDialogueNode();
    }

    // Add to DialogueManager.cs
    public static event Action OnDialogueSequenceComplete;

    // Call this when dialogue sequence ends
    private void CompleteDialogue()
    {
        isDialogueActive = false;
        if (toDestroy)
        {
            Destroy(zone);
        }
        OnDialogueSequenceComplete?.Invoke();
    }
}