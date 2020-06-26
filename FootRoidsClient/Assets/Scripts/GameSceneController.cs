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
    public GameObject dummyAsteroid;

    [Header("HUD Settings")]
    [Space]
    private int totalPoints;
    private HUDController hUDController;
    //private PlayerController player;

    [Header("Player Settings")]
    [Space]
    [SerializeField] GameObject[] playerShipPrefabsBlue;
    [SerializeField] GameObject[] playerShipPrefabsRed;

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
    
    readonly Dictionary<int, GameObject> playerShips = new Dictionary<int, GameObject>();
    readonly Dictionary<int, GameObject> activeAsteroids = new Dictionary<int, GameObject>();
    readonly Dictionary<int, GameObject> activeBalls = new Dictionary<int, GameObject>();

    //public GameObject[] teams;
    //public GameObject teamObjectPrefab;

    void Start()
    {
        hUDController = FindObjectOfType<HUDController>();
        screenBounds = GetScreenBounds();

        MatchCommunicationManager.Instance.OnAsteroidSpawned += OnSpawnAsteroid;
        MatchCommunicationManager.Instance.OnPlayerSpawned += OnSpawnPlayers;
        MatchCommunicationManager.Instance.OnBallSpawned += OnSpawnBalls;
        MatchCommunicationManager.Instance.OnGoalSpawned += OnSpawnGoals;

        if (MatchMaker.Instance.IsHost)
        {
            MatchCommunicationManager.Instance.OnPlayerInputRotationUpdated += PlayerInputRotationUpdated;
            MatchCommunicationManager.Instance.OnPlayerInputThrustUpdated += PlayerInputThrustUpdated;
        }
        else
        {
            MatchCommunicationManager.Instance.OnPlayerPositionUpdated += PlayerPositionUpdated;
            MatchCommunicationManager.Instance.OnAsteroidPositionUpdated += AsteroidPositionUpdated;
            MatchCommunicationManager.Instance.OnBallPositionUpdated += BallPositionUpdated;
        }

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
        SpawnBalls();
        //SpawnGoals();
    }

    // Spawn Asteroids in random locations  #Needs work not visible yet
    private void SpawnAsteroids()
    {
        for (int currentAsteroids = 0; currentAsteroids < maxAsteroids; currentAsteroids++)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);

            MatchMessageSpawnElement element = new MatchMessageSpawnElement(currentAsteroids.ToString(), horizontalPosition,
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
        bool teamAssignment = false;
        int playerIndex = 0;

        foreach (var player in MatchMaker.Instance.ReadyPlayers)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);

            MatchMessageSpawnShip element = new MatchMessageSpawnShip(player.UserId, teamAssignment, playerIndex, horizontalPosition,
                verticalPosition, 0.0f);

            // tell the clients
            MatchCommunicationManager.Instance.SendMatchStateMessage(
                MatchMessageType.PlayerSpawned, element);

            // tell yourself (host)
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(
                MatchMessageType.PlayerSpawned, element);

            teamAssignment = !teamAssignment;
            playerIndex++;
            playerIndex /= 2;
        }
    }

    // Spawn Balls in sides of the field ##Needs Fixing
    private void SpawnBalls()
    {
        for (int currentBalls = 0; currentBalls < numOfBalls; currentBalls++)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);

            MatchMessageSpawnElement element = new MatchMessageSpawnElement(currentBalls.ToString(), horizontalPosition,
                verticalPosition, 0.0f);

            // tell the clients
            MatchCommunicationManager.Instance.SendMatchStateMessage(
                MatchMessageType.BallSpawned, element);

            // tell yourself (host)
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(
                MatchMessageType.BallSpawned, element);
        };
    }

    // Spawn Goals in sides of the field ##Needs Fixing
    private void SpawnGoals()
    {
        // Spawn Left Goal
        float horizontalPosition = -screenBounds.x + goalOffset;
        float verticalPosition = 0;

        MatchMessageSpawnElement element = new MatchMessageSpawnElement("GoalLeft", horizontalPosition,
                verticalPosition, 0.0f);

        // tell the clients
        MatchCommunicationManager.Instance.SendMatchStateMessage(
            MatchMessageType.GoalSpawned, element);

        // tell yourself (host)
        MatchCommunicationManager.Instance.SendMatchStateMessageSelf(
            MatchMessageType.GoalSpawned, element);

        // Spawn Right Goal
        horizontalPosition = screenBounds.x - goalOffset;
        verticalPosition = 0;

        element = new MatchMessageSpawnElement("GoalRight", horizontalPosition,
                verticalPosition, 180.0f);

        // tell the clients
        MatchCommunicationManager.Instance.SendMatchStateMessage(
            MatchMessageType.GoalSpawned, element);

        // tell yourself (host)
        MatchCommunicationManager.Instance.SendMatchStateMessageSelf(
            MatchMessageType.GoalSpawned, element);
    }

    private void OnSpawnAsteroid(MatchMessageSpawnElement message)
    {
        Debug.Log("Spawning asteroid");
        
        try
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Quaternion rot = Quaternion.AngleAxis(message.angle, Vector3.forward);
                Vector3 pos = new Vector3(message.x, message.y);

                GameObject roid;
                if(MatchMaker.Instance.IsHost)
                {
                    roid = Instantiate(asteroid, pos, rot, transform);
                    roid.GetComponent<AsteroidScript>().id = message.elementId;
                }
                else
                {
                    roid = Instantiate(dummyAsteroid, pos, rot, transform);
                    roid.GetComponent<DummyController>().id = message.elementId;
                }                

                activeAsteroids.Add(message.elementId, roid);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void OnSpawnPlayers(MatchMessageSpawnShip message)
    {
        Debug.Log("Spawning Player");

        // TODO: more details on setting orientation...can we send a transform?
        // TODO: handle destruction
        // TODO: need to keep track in a list?            

        try
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Quaternion rot = Quaternion.AngleAxis(message.angle, Vector3.forward);
                Vector3 pos = new Vector3(message.x, message.y);

                GameObject prefab = message.team ? playerShipPrefabsRed[message.playerIndex] : playerShipPrefabsBlue[message.playerIndex];
                
                var shipInstance = Instantiate(prefab, pos, rot, transform);
                shipInstance.GetComponent<PlayerController>().SetTeamAndId(message.team, message.elementId);
                playerShips.Add(message.elementId, shipInstance);
                
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void OnSpawnBalls(MatchMessageSpawnElement message)
    {
        Debug.Log("Spawning Balls");

        // TODO: more details on setting orientation...can we send a transform?
        // TODO: handle destruction
        // TODO: need to keep track in a list?            

        try
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Quaternion rot = Quaternion.AngleAxis(message.angle, Vector3.forward);
                Vector3 pos = new Vector3(message.x, message.y);
                
                GameObject ball = Instantiate(ballObjectPrefab, pos, rot, transform);
                ball.GetComponent<BallScript>().id = message.elementId;
                
                activeBalls.Add(message.elementId, ball);

            });
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void OnSpawnGoals(MatchMessageSpawnElement message)
    {
        Debug.Log("Spawning Goals");

        // TODO: more details on setting orientation...can we send a transform?
        // TODO: handle destruction
        // TODO: need to keep track in a list?            

        try
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Quaternion rot = Quaternion.AngleAxis(message.angle, Vector3.forward);
                Vector3 pos = new Vector3(message.x, message.y);
                GameObject roid = Instantiate(goalObjectPrefab, pos, rot, transform);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    // Get the screen bounds
    private Vector3 GetScreenBounds()
    {
        Camera mainCamera = Camera.main;
        Vector3 screenVector = new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z);

        return mainCamera.ScreenToWorldPoint(screenVector);
    }

    void PlayerPositionUpdated(float posX, float posY, float angle, int shipId)
    {
        playerShips.TryGetValue(shipId, out GameObject ship);
        if (ship != null)
        {
            ship.transform.position = new Vector3(posX, posY, 0.0f);
            
            var newRot = ship.transform.eulerAngles;
            newRot.z = angle;
            
            ship.transform.eulerAngles = newRot;
        }
    }

    void PlayerInputRotationUpdated(float rotation, int shipId)
    {
        playerShips.TryGetValue(shipId, out GameObject ship);
        if (ship != null)
        {
            ship.GetComponent<PlayerController>().turnInput = rotation;
        }
    }
    
    void PlayerInputThrustUpdated(float thrust, int shipId)
    {
        playerShips.TryGetValue(shipId, out GameObject ship);
        if (ship != null)
        {
            ship.GetComponent<PlayerController>().thrustInput = thrust;
        }
    }

    void AsteroidPositionUpdated(int id, float x, float y)
    {
        activeAsteroids.TryGetValue(id, out GameObject roid);
        if(roid != null)
        {
            roid.transform.position = new Vector3(x, y, 0);
        }
    }
    
    void BallPositionUpdated(float x, float y, float angle, int id)
    {
        activeBalls.TryGetValue(id, out GameObject ball);
        if(ball != null)
        {
            ball.transform.position = new Vector3(x, y, 0.0f);
            
            var newRot = ball.transform.eulerAngles;
            newRot.z = angle;
            
            ball.transform.eulerAngles = newRot;
        }
    }
}
