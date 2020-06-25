using System.Collections.Generic;
using Multiplayer;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.WSA;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public SpriteRenderer sr;

    public float thrust;
    public float turnThrust;
    public float deathSpeed;
    public float mediumSpeed;
    public float maxSpeed;

    Vector3 m_PreviousPosition;
    float m_PreviousRotation;
    
    // Access the GameSceneController
    public GameSceneController gameSceneController;

    public int id { get; set; }
    
    public float thrustInput { get; set; }
    public float turnInput { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        gameSceneController = FindObjectOfType<GameSceneController>();
        rb = GetComponentInParent<Rigidbody2D>();
        sr = GetComponentInParent<SpriteRenderer>();

        var trs = transform;
        m_PreviousPosition = trs.position;
        m_PreviousRotation = trs.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeColor();
        MovePlayer();
    }

    // Fixed timing update
    void FixedUpdate()
    {
        if (!MatchMaker.Instance.IsHost)
        {
            return;
        }
        
        var trs = transform;
        var pos = trs.position;
        var rot = trs.eulerAngles.z;
        
        if (m_PreviousPosition != pos || m_PreviousRotation != rot)
        {
            var opCode = MatchMessageType.PlayerPositionUpdated;
            var newState = new MatchMessagePositionUpdated(id, pos.x, pos.y, rot);
            MatchCommunicationManager.Instance.SendMatchStateMessage(opCode, newState);
        }

        m_PreviousPosition = pos;
        m_PreviousRotation = rot;

        // Get input and apply thrust
        //thrustInput = Input.GetAxis("Vertical");
        rb.AddRelativeForce(Vector2.up * thrustInput * thrust);
        //rb.AddTorque(-turnInput * turnThrust);
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    private void ChangeColor()
    {
        //change speed color
        if (rb.velocity.magnitude > mediumSpeed && rb.velocity.magnitude < deathSpeed)
        {
            sr.color = Color.yellow;
        }
        else if (rb.velocity.magnitude > deathSpeed)
        {
            sr.color = Color.red;
        }
        else
        {
            sr.color = Color.white;
        }
    }

    private void MovePlayer()
    {
        // Get input and apply thrust
        //turnInput = Input.GetAxis("Horizontal");

        //rotate the ship 
        transform.Rotate(Vector3.forward * -turnInput * Time.deltaTime * turnThrust);

        return;
        
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

    private void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("relative hit: " + col.relativeVelocity.magnitude);
        if(col.relativeVelocity.magnitude > mediumSpeed) {
            Debug.Log("boom");
        }
    }

    public void RotatePlayer(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            var rotationInput = callbackContext.action.ReadValue<Vector2>();

            if (MatchMaker.Instance.IsHost)
            {
                turnInput = rotationInput.x;
            }
            else
            {
                var opCode = MatchMessageType.PlayerInputRotationUpdated;
                var newState = new MatchMessageInputRotationUpdated(ServerSessionManager.Instance.Session.UserId, rotationInput.x);
                MatchCommunicationManager.Instance.SendMatchStateMessage(opCode, newState);
            }
        }
        else if (callbackContext.canceled)
        {
            if (MatchMaker.Instance.IsHost)
            {
                turnInput = 0.0f;
            }
            else
            {
                var opCode = MatchMessageType.PlayerInputRotationUpdated;
                var newState = new MatchMessageInputRotationUpdated(ServerSessionManager.Instance.Session.UserId, 0.0f);
                MatchCommunicationManager.Instance.SendMatchStateMessage(opCode, newState);
            }
            
            turnInput = 0.0f;
        }
    }

    public void ThrustPlayer(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            var thrustInputValue = callbackContext.action.ReadValue<Vector2>();

            if (MatchMaker.Instance.IsHost)
            {
                thrustInput = thrustInputValue.y;
            }
            else
            {
                var opCode = MatchMessageType.PlayerInputThrustUpdated;
                var newState = new MatchMessageInputThrustUpdated(ServerSessionManager.Instance.Session.UserId, thrustInputValue.y);
                MatchCommunicationManager.Instance.SendMatchStateMessage(opCode, newState);
            }
        }
        else if (callbackContext.canceled)
        {
            if (MatchMaker.Instance.IsHost)
            {
                thrustInput = 0.0f;
            }
            else
            {
                var opCode = MatchMessageType.PlayerInputThrustUpdated;
                var newState = new MatchMessageInputThrustUpdated(ServerSessionManager.Instance.Session.UserId, 0.0f);
                MatchCommunicationManager.Instance.SendMatchStateMessage(opCode, newState);
            }
        }
    }
}
