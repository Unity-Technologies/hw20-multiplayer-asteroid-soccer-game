using Multiplayer;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    [HideInInspector] public int id;
    
    public GameSceneController gameSceneController;

    void Start()
    {
        gameSceneController = FindObjectOfType<GameSceneController>();
    }

    void Update()
    {
        CapMovement();
        SendUpdate();
    }

    private void CapMovement()
    {
        // Screen wrapping
        Vector2 newPos = transform.position;

        // Check if the Asteroid has moved out of screenBounds
        if (transform.position.y > gameSceneController.screenBounds.y)
        {
            newPos.y = -gameSceneController.screenBounds.y;
        }
        if (transform.position.y < -gameSceneController.screenBounds.y)
        {
            newPos.y = gameSceneController.screenBounds.y;
        }

        if (transform.position.x > gameSceneController.screenBounds.x)
        {
            newPos.x = -gameSceneController.screenBounds.x;
        }
        if (transform.position.x < -gameSceneController.screenBounds.x)
        {
            newPos.x = gameSceneController.screenBounds.x;
        }

        // Set the position back to the transform
        transform.position = newPos;
    }
    
    void SendUpdate()
    {
        var message = new MatchMessageBallPositionUpdated(id, transform.position.x, transform.position.y, transform.rotation.z);
        MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.BallPositionUpdated, message);
    }
}
