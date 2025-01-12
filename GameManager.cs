using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string nextSpawnPointName = "DefaultSpawn"; // Default spawn point name

    [Header("Game Over Settings")]
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float blackScreenDuration = 0.5f;  // How long to stay black
    [SerializeField] private Image blackoutImage;
    //[SerializeField] private Transform checkpointPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var canvas = GameObject.Find("FadeToBlack").GetComponent<Image>();
        blackoutImage = canvas;
    }
    public void SetNextSpawnPoint(string spawnPointName)
    {
        nextSpawnPointName = spawnPointName;
    }

    public void OnPlayerCaught(Action onBlackScreen = null)
    {
        StartCoroutine(HandlePlayerCaught(onBlackScreen));
    }

    private IEnumerator HandlePlayerCaught(Action onBlackScreen)
    {
        // Fade to black
        float elapsed = 0;
        Color color = blackoutImage.color;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsed / fadeOutDuration);
            blackoutImage.color = color;
            yield return null;
        }

        // Screen is now black - execute callback for enemy reset
        onBlackScreen?.Invoke();

        // Reset player position using checkpoint system
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = CheckpointManager.Instance.GetCurrentCheckpointPosition();
        }

        // Stay black for a moment
        yield return new WaitForSeconds(blackScreenDuration);

        // Fade back in
        elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
            blackoutImage.color = color;
            yield return null;
        }
    }
}