using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnHandler : MonoBehaviour
{
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
        // Find the spawn point by name
        string spawnPointName = GameManager.Instance.nextSpawnPointName;
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        if (spawnPoint != null)
        {
            // Move the player to the spawn point position
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation; // Optional: Align rotation
        }
        else
        {
            Debug.LogWarning($"Spawn point '{spawnPointName}' not found. Player remains at default position.");
        }
    }

    /*private void Start()
    {
        string spawnPointName = GameManager.Instance.nextSpawnPointName; // Get the spawn point name from GameManager

        // Find the spawn point in the scene by name
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position; // Set player position
            transform.rotation = spawnPoint.transform.rotation; // Optionally set rotation
        }
        else
        {
            Debug.LogWarning($"Spawn point '{spawnPointName}' not found. Player will spawn at default position.");
        }
    }*/
}
