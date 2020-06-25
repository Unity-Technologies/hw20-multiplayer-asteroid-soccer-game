using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    [Header("Screen Settings")]
    [Space]
    public Vector3 screenBounds;

    [Header("Multiplayer Settings")]
    [Space]
    public bool isMaster;

    [Header("Asteroid Settings")]
    [Space]
    public GameObject[] asteroids;
    public int maxAsteroids;
    public GameObject asteroid;

    [Header("HUD Settings")]
    [Space]
    private int totalPoints;
    private HUDController hUDController;
    //private PlayerController player;

    [Header("Player Settings")]
    [Space]
    public GameObject[] playerList;
    public GameObject playerObjectPrefab;
    public int numOfPlayers;

    [Header("Ball Settings")]
    [Space]
    public GameObject[] balls;
    public GameObject ballObjectPrefab;
    public int numOfBalls;

    //public GameObject[] teams;
    //public GameObject teamObjectPrefab;

    // Start is called before the first frame update
    void Start()
    {
        hUDController = FindObjectOfType<HUDController>();
        screenBounds = GetScreenBounds();
        StartCoroutine(SpawnAsteroids());
        StartCoroutine(SpawnPlayers());
        StartCoroutine(SpawnBalls());
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Spawn Asteroids in random locations  #Needs work not visible yet
    private IEnumerator SpawnAsteroids()
    {
        for (int currentAsteroids = 0; currentAsteroids < maxAsteroids; currentAsteroids++)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);
            Instantiate(asteroid, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);
            yield return true;
        };
    }

    // Spawn Players in sides of the field ##Needs Fixing
    private IEnumerator SpawnPlayers()
    {
        
        for (int currentPlayers = 0; currentPlayers < numOfPlayers; currentPlayers++)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);
            // instantiate a player
            Instantiate(playerObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);                
            yield return true;
        };
    }

    // Spawn Balls in sides of the field ##Needs Fixing
    private IEnumerator SpawnBalls()
    {

        for (int currentBalls = 0; currentBalls < numOfBalls; currentBalls++)
        {
            SpawnBall();
            yield return true;
        };
    }

    public void SpawnBall() {
        float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
        float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);
        Instantiate(ballObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);
    }


    // Get the screen bounds
    private Vector3 GetScreenBounds()
    {
        Camera mainCamera = Camera.main;
        Vector3 screenVector = new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z);

        return mainCamera.ScreenToWorldPoint(screenVector);
    }
}
