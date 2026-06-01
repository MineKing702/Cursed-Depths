using UnityEngine;

public sealed class SceneSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId = "Area2PlayerSpawn";

    public string SpawnId => spawnId;
}
