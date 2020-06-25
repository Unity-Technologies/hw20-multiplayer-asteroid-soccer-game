using System.Collections;
using System.Collections.Generic;
using Multiplayer;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : Singleton<GameSceneController>
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

    void Start()
    {
        MatchCommunicationManager.Instance.OnAsteroidSpawned += OnSpawnAsteroid;
        MatchCommunicationManager.Instance.OnPlayerSpawned += OnSpawnPlayers;

        if (MatchMaker.Instance.IsHost)
        {
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.StadiumEntered,
                new MatchMessageStadiumEntered(ServerSessionManager.Instance.Session.UserId));
        }
        else
        {
            MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.StadiumEntered,
                new MatchMessageStadiumEntered(ServerSessionManager.Instance.Session.UserId));
        }
    }

    public void InitializeGame()
    {
        if (MatchMaker.Instance.IsHost == false)
        {
            Debug.LogError("NOT HOST");
            return;
        }

        Debug.Log("Populating Game Elements");

        SpawnAsteroids();
        SpawnPlayers();
    }

    // Spawn Asteroids in random locations  #Needs work not visible yet
    private void SpawnAsteroids()
    {
        for (int currentAsteroids = 0; currentAsteroids < maxAsteroids; currentAsteroids++)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);

            MatchMessageSpawnElement element = new MatchMessageSpawnElement(0, horizontalPosition,
                verticalPosition, 0.0f);

            // tell the clients
            MatchCommunicationManager.Instance.SendMatchStateMessage(
                MatchMessageType.AsteroidSpawned, element);

            // tell yourself (host)
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(
                MatchMessageType.AsteroidSpawned, element);
        };
    }

    // Spawn Players in sides of the field ##Needs Fixing
    private void SpawnPlayers()
    {
        for (int currentPlayers = 0; currentPlayers < numOfPlayers; currentPlayers++)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);


            MatchMessageSpawnElement element = new MatchMessageSpawnElement(0, horizontalPosition,
                verticalPosition, 0.0f);

            // tell the clients
            MatchCommunicationManager.Instance.SendMatchStateMessage(
                MatchMessageType.PlayerSpawned, element);

            // tell yourself (host)
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(
                MatchMessageType.PlayerSpawned, element);           
        };
    }

    private void OnSpawnAsteroid(MatchMessageSpawnElement message)
    {
        Debug.Log("Spawning asteroid");

        // TODO: more details on setting orientation...can we send a transform?
        // TODO: handle destruction
        // TODO: need to keep track in a list?            

        try
        {
            Quaternion rot = Quaternion.AngleAxis(message.angle, Vector3.forward);

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Vector3 pos = new Vector3(message.x, message.y);
                GameObject roid = Instantiate(asteroid, pos, rot, transform);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }    

    // Spawn Balls in sides of the field ##Needs Fixing
    private void SpawnBalls()
    {
        for (int currentBalls = 0; currentBalls < numOfBalls; currentBalls++)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);
            // instantiate a ball
            Instantiate(ballObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);
        };
    }

    private void OnSpawnPlayers(MatchMessageSpawnElement message)
    {
        Debug.Log("Spawning Player");

        // TODO: more details on setting orientation...can we send a transform?
        // TODO: handle destruction
        // TODO: need to keep track in a list?            

        try
        {
            Quaternion rot = Quaternion.AngleAxis(message.angle, Vector3.forward);

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Vector3 pos = new Vector3(message.x, message.y);
                GameObject roid = Instantiate(playerObjectPrefab, pos, rot, transform);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void OnSpawnBalls()
    {

    }

    private void OnSpawnGoals()
    {

    }    

    // Spawn Goals in sides of the field ##Needs Fixing
    private void SpawnGoals()
    {
        // Spawn Left Goal
        float horizontalPosition = -screenBounds.x + goalOffset;
        float verticalPosition = 0;
        Instantiate(goalObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);

        // Spawn Right Goal
        horizontalPosition = screenBounds.x - goalOffset;
        verticalPosition = 0;
        //Instantiate(goalObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);
        var rightGoal = Instantiate(goalObjectPrefab, new Vector2(horizontalPosition, verticalPosition), Quaternion.identity);

        rightGoal.GetComponent<SpriteRenderer>();
        // Flip the goal
        rightGoal.transform.localScale = new Vector3(-1, 1, 1);
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
        //m_TempOtherShipInstance.transform.position = pos;
    }
    
    void RotationUpdated(float rot, int shipId)
    {
        //var newRot = m_TempOtherShipInstance.transform.eulerAngles;
        //newRot.z = rot;
        
        //m_TempOtherShipInstance.transform.eulerAngles = newRot;
    }
}
