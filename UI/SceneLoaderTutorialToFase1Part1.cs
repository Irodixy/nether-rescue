using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoaderTutorialToFase1Part1 : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // Reference to the UI Image component
    [SerializeField] private float fadeDuration = 1f; // Duration of the fade in seconds

    public string nextSceneName;
    public string spawnPointName; // Name of the spawn point in the next scene

    // Start is called before the first frame update
    void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image is not assigned. Please assign it in the inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        // Subscribe to dialogue completion event
        TutorialManager.OnTutorialCompleted += PrepareFadeToBlack;
    }

    private void OnDisable()
    {
        // Unsubscribe to dialogue completion event
        TutorialManager.OnTutorialCompleted -= PrepareFadeToBlack;
    }

    private void PrepareFadeToBlack()
    {
        StartCoroutine(FadeToBlackCoroutine());
    }

    private void ChangeScene()
    {
        // Set the next spawn point in GameManager
        GameManager.Instance.SetNextSpawnPoint(spawnPointName);
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeToBlackCoroutine()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // Fully opaque

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeImage.color = endColor; // Ensure it finishes at full black
        ChangeScene();
    }
}
