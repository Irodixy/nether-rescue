using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image interactionIcon;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 8f;

    private CanvasGroup canvasGroup;
    private bool shouldShow;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupComponents()
    {
        // If interactionPanel not set, use this GameObject
        if (interactionPanel == null)
            interactionPanel = gameObject;

        // Get or add CanvasGroup
        canvasGroup = interactionPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = interactionPanel.AddComponent<CanvasGroup>();

        // Verify TextMeshProUGUI component
        if (promptText == null)
        {
            promptText = GetComponentInChildren<TextMeshProUGUI>();
            if (promptText == null)
                Debug.LogError("No TextMeshProUGUI component found for interaction prompt!");
        }
    }

    private void Start()
    {
        HidePrompt();
    }

    private void Update()
    {
        if (canvasGroup == null) return;

        // Smooth fade in/out
        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Disable panel completely if nearly invisible
        if (!shouldShow && canvasGroup.alpha < 0.01f)
            interactionPanel.SetActive(false);
    }

    public void ShowPrompt(string promptMessage)
    {
        if (interactionPanel == null || promptText == null)
        {
            Debug.LogError("InteractionUI components not properly set up!");
            return;
        }

        interactionPanel.SetActive(true);
        promptText.text = promptMessage;
        shouldShow = true;
    }

    public void HidePrompt()
    {
        shouldShow = false;
    }
}