﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Multiplayer;

public class AsteroidScript : MonoBehaviour
{

    public float maxThrust;
    public float maxTorque;

    // Access GameSceneController
    public GameSceneController gameSceneController;
    public Rigidbody2D rb;
    public Collider2D roidCollider;

    public int id;
    
    // Start is called before the first frame update
    void Start()
    {
        gameSceneController = FindObjectOfType<GameSceneController>();

        //add a random ammount of torque and thurst to apply to asteroids
        Vector2 thrust = new Vector2(Random.Range(-maxThrust,maxThrust), Random.Range(-maxThrust,maxThrust));
        float torque = Random.Range(-maxTorque,maxTorque);

        rb.AddForce(thrust);
        rb.AddTorque(torque);        
    }

    // HOST
    void Update()
    {       
        MoveAsteroid();
        SendUpdate();
    }
   
    private void MoveAsteroid()
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
        var message = new MatchMessageAsteroidPositionUpdated(id, transform.position.x, transform.position.y);

        MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.AsteroidPositionUpdated, message);
    }
}
