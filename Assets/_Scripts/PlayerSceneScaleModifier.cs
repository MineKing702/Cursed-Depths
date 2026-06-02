using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Applies scene-specific player scale and movement tuning for oversized rooms.
/// </summary>
public sealed class PlayerSceneScaleModifier : MonoBehaviour
{
    private const string PuzzleRoomSceneName = "Puzzle Room";

    [SerializeField] private float scaleMultiplier = 2f;
    [SerializeField] private float moveSpeedMultiplier = 1.5f;
    [SerializeField] private float jumpForceMultiplier = 1.5f;

    private PlayerController playerController;
    private Vector3 originalLocalScale;

    private void Awake()
    {
        originalLocalScale = transform.localScale;
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;
        ApplySceneTuning(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene currentScene)
    {
        ApplySceneTuning(currentScene);
    }

    private void ApplySceneTuning(Scene activeScene)
    {
        if (activeScene.name == PuzzleRoomSceneName)
        {
            ApplyPuzzleRoomTuning();
            return;
        }

        ResetNormalTuning();
    }

    private void ApplyPuzzleRoomTuning()
    {
        if (playerController != null)
        {
            playerController.SetSceneScaleMultiplier(scaleMultiplier);
            playerController.ApplyMovementMultiplier(moveSpeedMultiplier, jumpForceMultiplier);
            return;
        }

        transform.localScale = originalLocalScale * scaleMultiplier;
    }

    private void ResetNormalTuning()
    {
        if (playerController != null)
        {
            playerController.ResetSceneScaleMultiplier();
            playerController.ResetMovementTuning();
            return;
        }

        transform.localScale = originalLocalScale;
    }
}
