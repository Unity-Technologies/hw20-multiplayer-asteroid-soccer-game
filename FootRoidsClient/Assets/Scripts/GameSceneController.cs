﻿using System.Collections;
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
    [SerializeField] GameObject playerObjectPrefab;
    [SerializeField] GameObject playerObjectPrefab_Simulation;
    [SerializeField] GameObject playerObjectPrefab_Network;
    [SerializeField] GameObject playerObjectPrefab_Network_Simulation;
    [SerializeField] int numOfPlayers;

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
    
    Dictionary<int, GameObject> otherPlayers = new Dictionary<int, GameObject>();

    void Start()
    {
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
        SpawnGoals();
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
        foreach (var player in MatchMaker.Instance.ReadyPlayers)
        {
            float horizontalPosition = Random.Range(-screenBounds.x, screenBounds.x);
            float verticalPosition = Random.Range(-screenBounds.y, screenBounds.y);

            MatchMessageSpawnElement element = new MatchMessageSpawnElement(player.UserId, horizontalPosition,
                verticalPosition, 0.0f);

            // tell the clients
            MatchCommunicationManager.Instance.SendMatchStateMessage(
                MatchMessageType.PlayerSpawned, element);

            // tell yourself (host)
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(
                MatchMessageType.PlayerSpawned, element);
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

        // TODO: more details on setting orientation...can we send a transform?
        // TODO: handle destruction
        // TODO: need to keep track in a list?            

        try
        {

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Quaternion rot = Quaternion.AngleAxis(message.angle, Vector3.forward);
                Vector3 pos = new Vector3(message.x, message.y);
                GameObject roid = Instantiate(asteroid, pos, rot, transform);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void OnSpawnPlayers(MatchMessageSpawnElement message)
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

                if (MatchMaker.Instance.IsHost)
                {
                    if (message.elementId == ServerSessionManager.Instance.Session.UserId.GetHashCode())
                    {
                        // Full input, full collisions
                        var prefab = Instantiate(playerObjectPrefab, pos, rot, transform);
                        prefab.GetComponent<PlayerController>().id = message.elementId;
                    }
                    else
                    {
                        // No input, full collisions
                        var prefab = Instantiate(playerObjectPrefab_Simulation, pos, rot, transform);
                        prefab.GetComponent<PlayerController>().id = message.elementId;
                        otherPlayers.Add(message.elementId, prefab);
                    }
                }
                else
                {
                    if (message.elementId == ServerSessionManager.Instance.Session.UserId.GetHashCode())
                    {
                        // Full input, no collisions
                        Instantiate(playerObjectPrefab_Network, pos, rot, transform);
                    }
                    else
                    {
                        // No input, no collisions
                        otherPlayers.Add(message.elementId, Instantiate(playerObjectPrefab_Network_Simulation, pos, rot, transform));
                    }
                }
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
                GameObject roid = Instantiate(ballObjectPrefab, pos, rot, transform);
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
        Debug.LogError("HEREEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        otherPlayers.TryGetValue(shipId, out GameObject ship);
        if (ship != null)
        {
            Debug.LogError("SSSSSSSSSSSSSSSSSSSSSSSSSSSSS");
            ship.transform.position = new Vector3(posX, posY, 0.0f);
            
            var newRot = ship.transform.eulerAngles;
            newRot.z = angle;
            
            ship.transform.eulerAngles = newRot;
        }
    }

    void PlayerInputRotationUpdated(float rotation, int shipId)
    {
        otherPlayers.TryGetValue(shipId, out GameObject ship);
        if (ship != null)
        {
            ship.GetComponent<PlayerController>().turnInput = rotation;
        }
    }
    
    void PlayerInputThrustUpdated(float thrust, int shipId)
    {
        otherPlayers.TryGetValue(shipId, out GameObject ship);
        if (ship != null)
        {
            ship.GetComponent<PlayerController>().thrustInput = thrust;
        }
    }
}
