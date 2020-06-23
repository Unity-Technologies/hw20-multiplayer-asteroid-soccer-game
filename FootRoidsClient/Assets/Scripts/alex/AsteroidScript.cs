using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidScript : MonoBehaviour
{

    public float maxThrust;
    public float maxTorque;

    //Screen maximums
    public float screenTop;
    public float screenBottom;
    public float screenRight;
    public float screenLeft;


    public Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
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
        if(transform.position.y > screenTop){
            newPos.y = screenBottom;
        }
        if(transform.position.y < screenBottom){
            newPos.y = screenTop;
        }
    
        if(transform.position.x > screenRight){
            newPos.x = screenLeft;
        }
        if(transform.position.x < screenLeft){
            newPos.x = screenRight;
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
