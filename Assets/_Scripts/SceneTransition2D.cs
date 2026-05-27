using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition2D : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Area2";
    [SerializeField] private string targetSpawnPointName = "player spawn from area 1";
    [SerializeField] private string playerTag = "Player";

    private bool isTransitioning;
    private GameObject persistentPlayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTransition(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTransition(collision.gameObject);
    }

    private void TryTransition(GameObject collidedObject)
    {
        if (isTransitioning || collidedObject == null || !collidedObject.CompareTag(playerTag))
        {
            return;
        }

        persistentPlayer = collidedObject.transform.root.gameObject;
        isTransitioning = true;

        DontDestroyOnLoad(persistentPlayer);
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        if (persistentPlayer == null)
        {
            isTransitioning = false;
            return;
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject player in players)
        {
            if (player != persistentPlayer)
            {
                Destroy(player);
            }
        }

        GameObject spawnPoint = GameObject.Find(targetSpawnPointName);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"SceneTransition2D: Spawn point '{targetSpawnPointName}' was not found in scene '{scene.name}'.");
            isTransitioning = false;
            return;
        }

        Transform playerTransform = persistentPlayer.transform;
        playerTransform.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);

        Rigidbody2D playerBody = persistentPlayer.GetComponent<Rigidbody2D>();
        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
        }

        isTransitioning = false;
    }
}
