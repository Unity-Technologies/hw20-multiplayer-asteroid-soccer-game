using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCenterController : MonoBehaviour
{
    private AudioSource audioSource;
    public ScoreScript ScoreText;
    public GameSceneController GameScene;
    private int goalScore = 0;
    public GameObject explosionPrefab;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.gameObject.tag.Equals("Ball"))
        {
            goalScore = goalScore + 1;
            ScoreText.SetScore(goalScore);
            // play score sound 
            audioSource.Play();

            // show explosion gameObject
            var explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
            Destroy(explosion, 4);

            Destroy (collision.gameObject); // Destroy the colliding socccer ball
            //GameScene.SpawnBall();
        }
    }
}
