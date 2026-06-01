using System.Collections;
using UnityEngine;

public sealed class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;

    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private float fadeDuration = 0.75f;

    private bool isTransitioning;

    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                SceneTransitionManager existingManager = FindFirstObjectByType<SceneTransitionManager>();
                if (existingManager != null)
                {
                    instance = existingManager;
                }
                else
                {
                    GameObject managerObject = new GameObject("SceneTransitionManager");
                    instance = managerObject.AddComponent<SceneTransitionManager>();
                }
            }

            return instance;
        }
    }

    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureFader();
    }

    public void TransitionToScene(string targetSceneName, string targetSpawnId, PlayerController player = null)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(TransitionRoutine(targetSceneName, targetSpawnId, player));
    }

    private IEnumerator TransitionRoutine(string targetSceneName, string targetSpawnId, PlayerController player)
    {
        isTransitioning = true;

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player != null)
        {
            player.SetControlEnabled(false);
            DontDestroyOnLoad(player.gameObject);
        }

        EnsureFader();
        yield return screenFader.FadeOut(fadeDuration);

        AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetSceneName);
        if (loadOperation != null)
        {
            while (!loadOperation.isDone)
            {
                yield return null;
            }
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player != null)
        {
            SceneSpawnPoint spawnPoint = MovePlayerToSpawn(player, targetSpawnId);
            ReconnectCameraToPlayer(player.transform, spawnPoint);
        }

        yield return screenFader.FadeIn(fadeDuration);

        if (player != null)
        {
            player.SetControlEnabled(true);
        }

        isTransitioning = false;
    }

    private SceneSpawnPoint MovePlayerToSpawn(PlayerController player, string targetSpawnId)
    {
        Transform spawnTransform = FindSpawnTransform(targetSpawnId);
        SceneSpawnPoint spawnPoint = spawnTransform != null ? spawnTransform.GetComponent<SceneSpawnPoint>() : null;

        if (spawnTransform == null)
        {
            Debug.LogWarning($"Scene transition could not find spawn point '{targetSpawnId}'. Player was left at the loaded scene origin fallback.", this);
            player.transform.position = Vector3.zero;
        }
        else
        {
            player.transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);

            if (spawnPoint != null)
            {
                ApplySpawnPointPlayerOverrides(player, spawnPoint);
            }
        }

        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }

        player.SetRespawnPoint(player.transform.position, player.transform.rotation);
        return spawnPoint;
    }

    private Transform FindSpawnTransform(string targetSpawnId)
    {
        SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (SceneSpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.SpawnId == targetSpawnId)
            {
                return spawnPoint.transform;
            }
        }

        GameObject namedSpawn = GameObject.Find(targetSpawnId);
        return namedSpawn != null ? namedSpawn.transform : null;
    }

    private static void ApplySpawnPointPlayerOverrides(PlayerController player, SceneSpawnPoint spawnPoint)
    {
        if (spawnPoint.OverridePlayerFacingScales)
        {
            player.SetFacingScales(spawnPoint.RightFacingScale, spawnPoint.LeftFacingScale, spawnPoint.FaceRightOnSpawn);
        }

        if (spawnPoint.OverridePlayerSortingOrder)
        {
            player.SetSpriteSortingOrder(spawnPoint.PlayerSortingOrder);
        }
    }

    private void ReconnectCameraToPlayer(Transform playerTransform, SceneSpawnPoint spawnPoint)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        if (spawnPoint != null && spawnPoint.OverrideCameraOrthographicSize && mainCamera.orthographic)
        {
            mainCamera.orthographicSize = spawnPoint.CameraOrthographicSize;
        }

        CameraMovement cameraMovement = mainCamera.GetComponent<CameraMovement>();
        if (cameraMovement == null)
        {
            cameraMovement = mainCamera.gameObject.AddComponent<CameraMovement>();
        }

        cameraMovement.SetTarget(playerTransform);
    }

    private void EnsureFader()
    {
        if (screenFader == null)
        {
            screenFader = GetComponentInChildren<ScreenFader>(true);
        }

        if (screenFader == null)
        {
            GameObject faderObject = new GameObject("ScreenFader");
            faderObject.transform.SetParent(transform, false);
            screenFader = faderObject.AddComponent<ScreenFader>();
        }
    }
}
