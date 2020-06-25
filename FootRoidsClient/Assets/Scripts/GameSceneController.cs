using System.Collections;
using System.Collections.Generic;
using Multiplayer;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Goal Settings")]
    [Space]
    public GameObject[] goals;
    public GameObject goalObjectPrefab;
    public int numOfGoals;
    public int goalOffset;

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
        StartCoroutine(SpawnGoals());

        m_TempOtherShipInstance = Instantiate(m_TempOtherShip, Vector3.zero, Quaternion.identity);
        m_TempOtherShipInstance.name = "OtherPlayer";
        SceneManager.MoveGameObjectToScene(m_TempOtherShipInstance, SceneManager.GetSceneByName("Stadium1"));
        
        MatchCommunicationManager.Instance.OnPositionUpdated += PositionUpdated;
        MatchCommunicationManager.Instance.OnRotationUpdated += RotationUpdated;
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
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);
            // instantiate a ball
            Instantiate(ballObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);
            yield return true;
        };
    }

    // Spawn Goals in sides of the field ##Needs Fixing
    private IEnumerator SpawnGoals()
    {
        // Spawn Left Goal
        float horizontalPosition = -screenBounds.x + goalOffset;
        float verticalPosition = 0;
        Instantiate(goalObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);

        // Spawn Right Goal
        horizontalPosition = screenBounds.x - goalOffset;
        verticalPosition = 0;
        Instantiate(goalObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);
        //var rightGoal = Instantiate(goalObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);

        // Flip the goal
        // rightGoal.sr.flip.x = true;

        yield return true;
    }

    // Get the screen bounds
    private Vector3 GetScreenBounds()
    {
        Camera mainCamera = Camera.main;
        Vector3 screenVector = new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z);

        return mainCamera.ScreenToWorldPoint(screenVector);
    }

    void PositionUpdated(Vector3 pos, int shipId)
    {
        m_TempOtherShipInstance.transform.position = pos;
    }
    
    void RotationUpdated(float rot, int shipId)
    {
        var newRot = m_TempOtherShipInstance.transform.eulerAngles;
        newRot.z = rot;
        
        m_TempOtherShipInstance.transform.eulerAngles = newRot;
    }
}
