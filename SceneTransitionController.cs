using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionController : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float blackScreenDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Loading Settings")]
    [SerializeField] private float minimumLoadingTime = 0.5f;

    private bool isTransitioning;
    public static SceneTransitionController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("FadeCanvasGroup not assigned, attempting to find one...");
            fadeCanvasGroup = GetComponentInChildren<CanvasGroup>();

            if (fadeCanvasGroup == null)
            {
                Debug.LogError("No CanvasGroup found! Scene transitions will not work!");
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress!");
            return;
        }

        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;

        // Fade to black
        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration));

        // Start scene loading
        float loadStartTime = Time.time;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait until the scene is loaded and minimum loading time has passed
        while (!asyncLoad.isDone || Time.time - loadStartTime < minimumLoadingTime)
        {
            if (asyncLoad.progress >= 0.9f && Time.time - loadStartTime >= minimumLoadingTime)
            {
                // Hold black screen
                yield return new WaitForSeconds(blackScreenDuration);
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // Ensure the scene is fully loaded before fading out
        yield return new WaitForSeconds(0.1f);

        // Fade back in
        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeOutDuration));

        isTransitioning = false;
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
    }
}