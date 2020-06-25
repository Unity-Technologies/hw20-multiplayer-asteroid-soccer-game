using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCenterController : MonoBehaviour
{
    public ScoreScript ScoreText;
    private int goalScore = 0;


    void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.gameObject.tag.Equals("Ball"))
        {
            goalScore = goalScore + 1;
            ScoreText.SetScore(goalScore);
            Destroy (collision.gameObject); // Destroy the colliding socccer ball
        }
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
