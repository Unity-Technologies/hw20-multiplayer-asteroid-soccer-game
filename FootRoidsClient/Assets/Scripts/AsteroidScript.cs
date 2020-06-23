using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidScript : MonoBehaviour
{

    public float maxThrust;
    public float maxTorque;

    // Access GameSceneController
    public GameSceneController gameSceneController;

    public Rigidbody2D rb;
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

    // Update is called once per frame
    void Update()
    {
        //screen wrapping
        Vector2 newPos =  transform.position;

        //begin if changes 
        if(transform.position.y > gameSceneController.screenTop)
        {
            newPos.y = gameSceneController.screenBottom;
        }
        if(transform.position.y < gameSceneController.screenBottom){
            newPos.y = gameSceneController.screenTop;
        }
    
        if(transform.position.x > gameSceneController.screenRight){
            newPos.x = gameSceneController.screenLeft;
        }
        if(transform.position.x < gameSceneController.screenLeft){
            newPos.x = gameSceneController.screenRight;
        }

        //set the position back to the transform
        transform.position = newPos;
    }

    void  OnTriggerEnter2D(Collider2D other) 
    {
        //Debug.Log("Hit by " +  other.name);
        //destroy the bullet 
        Destroy(other.gameObject);
    }
}
