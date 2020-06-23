using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;

    public SpriteRenderer sr;
    public float thrust;
    public float turnThrust;
    private float thrustInput;
    private float turnInput;

    // Access the GameSceneController
    public GameSceneController gameSceneController;

    public float deathSpeed;
    public float mediumSpeed;

    // Start is called before the first frame update
    void Start()
    {
        gameSceneController = FindObjectOfType<GameSceneController>();
        rb = GetComponentInParent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //get input from keyboard and apply thrust
        thrustInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        //rotate the ship 
        transform.Rotate(Vector3.forward * -turnInput * Time.deltaTime *turnThrust);
        
        //screen wrapping
        Vector2 newPos =  transform.position;

        //change speed color
        if (rb.velocity.magnitude > mediumSpeed && rb.velocity.magnitude < deathSpeed)
        {
            sr.color = Color.yellow;
        }
        else if (rb.velocity.magnitude > deathSpeed)
        {
            sr.color = Color.red;
        }
        else {
            sr.color = Color.white;
        }

        //begin if changes 
        if(transform.position.y > gameSceneController.screenTop)
        {
            newPos.y = gameSceneController.screenBottom;
        }
        if(transform.position.y < gameSceneController.screenBottom)
        {
            newPos.y = gameSceneController.screenTop;
        }
    
        if(transform.position.x > gameSceneController.screenRight)
        {
            newPos.x = gameSceneController.screenLeft;
        }
        if(transform.position.x < gameSceneController.screenLeft)
        {
            newPos.x = gameSceneController.screenRight;
        }

        //set it back
        transform.position = newPos;
    }

    //Fixed timeing update
    void FixedUpdate() {
        rb.AddRelativeForce(Vector2.up * thrustInput);
        //rb.AddTorque(-turnInput * turnThrust);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("relative hit: " + col.relativeVelocity.magnitude);
        if(col.relativeVelocity.magnitude > mediumSpeed) {
            Debug.Log("boom");
        }
    }
}
