using UnityEngine;

public sealed class SceneSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId = "Area2PlayerSpawn";
    [SerializeField] private bool overridePlayerFacingScales;
    [SerializeField] private Vector3 rightFacingScale = new Vector3(0.7f, 0.7f, 1f);
    [SerializeField] private Vector3 leftFacingScale = new Vector3(-0.7f, 0.7f, 1f);
    [SerializeField] private bool faceRightOnSpawn = true;
    [SerializeField] private bool overridePlayerMovement;
    [SerializeField] private float playerSpeed = 2f;
    [SerializeField] private float playerJumpForce = 3.75f;
    [SerializeField] private bool overridePlayerSortingOrder;
    [SerializeField] private int playerSortingOrder = 100;
    [SerializeField] private bool overrideCameraOrthographicSize;
    [SerializeField] private float cameraOrthographicSize = 3.5f;

    public string SpawnId => spawnId;
    public bool OverridePlayerFacingScales => overridePlayerFacingScales;
    public Vector3 RightFacingScale => rightFacingScale;
    public Vector3 LeftFacingScale => leftFacingScale;
    public bool FaceRightOnSpawn => faceRightOnSpawn;
    public bool OverridePlayerMovement => overridePlayerMovement;
    public float PlayerSpeed => playerSpeed;
    public float PlayerJumpForce => playerJumpForce;
    public bool OverridePlayerSortingOrder => overridePlayerSortingOrder;
    public int PlayerSortingOrder => playerSortingOrder;
    public bool OverrideCameraOrthographicSize => overrideCameraOrthographicSize;
    public float CameraOrthographicSize => cameraOrthographicSize;
}
