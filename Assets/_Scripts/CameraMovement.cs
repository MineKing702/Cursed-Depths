using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;

    // How far the player can move from the camera before it shifts
    public float horizontalThreshold = 5f;
    public float lowerThreshold = 3.5f;

    void Update()
    {
        Vector3 camPos = transform.position;

        // Player is too far right
        if (player.position.x > camPos.x + horizontalThreshold)
        {
            camPos.x = player.position.x - horizontalThreshold;
        }

        // Player is too far left
        else if (player.position.x < camPos.x - horizontalThreshold)
        {
            camPos.x = player.position.x + horizontalThreshold;
        }

        // Player is too far down
        else if (player.position.y < camPos.y - lowerThreshold)
        {
            camPos.y = player.position.y + lowerThreshold;
        }

        // Instantly move camera
        transform.position = camPos;
    }
}