using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Reference to the Player
    private Player player;
    // Offset between camera and player positions
    private Vector3 offset;

    void Start()
    {
        // Find the Player in the scene
        player = FindObjectOfType<Player>();
        if (player != null)
            offset = transform.position - player.transform.position;
        else
            Debug.LogWarning("CameraController: no Player found in scene");
    }

    void LateUpdate()
    {
        // Follow the player with the initial offset
        if (player != null)
            transform.position = player.transform.position + offset;
    }
}
