using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DialogueTriggerZone : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private float extraReadingTime = 2f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    public DialogueNode[] dialogueNodes;
    public float triggerRadius = 2f;

    private DialogueManager dialogueManager;

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTrigger(other))
        {
            SetupText();
            TriggerDialogue();
        }
    }

    private bool IsValidTrigger(Collider other)
    {
        // Check if the collider is the player
        if (!other.CompareTag("Player"))
            return false;  

        return true;
    }

    private void SetupText()
    {
        CalculateDisplayTime(dialogueNodes);
    }

    private void TriggerDialogue()
    {
        if (dialogueManager != null)
        {
            dialogueManager.dialogueNodes = dialogueNodes;
            dialogueManager.StartDialogue(gameObject);
        }
    }

    // Calculates display time based on text length
    private void CalculateDisplayTime(DialogueNode[] nodes)
    {
        // Split words and calculate reading time
        foreach (DialogueNode node in nodes)
        {
            string[] words = node.text.Split(' ');
            float minutes = words.Length / 200f;

            // Extract whole minutes and decimal
            int wholeMinutes = Mathf.FloorToInt(minutes);
            float decimalPart = minutes - wholeMinutes;

            // Convert decimal to seconds (multiply by 0.6) and then convert to unitys
            float additionalSeconds = decimalPart * 0.6f * 100f;

            // Add whole minutes, additional seconds, and extra reading time
            node.displayDuration = (wholeMinutes * 60f) + additionalSeconds + extraReadingTime;
        } 
    }

    // Optional: Visualize trigger zone in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
