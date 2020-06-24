using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Multiplayer;
using Nakama;
using UnityEngine;

public class GameManager : Singleton<GameManager> {

    [SerializeField] private GameObject _shipPrefab;
    private Dictionary<string, GameObject> _networkedGameObjects = new Dictionary<string, GameObject>();

    private void Start()
    {
        MatchCommunicationManager.Instance.OnGameEnded += EndGame;

        if(MatchCommunicationManager.Instance.GameStarted)
        {
            Debug.Log("Game already started, initializing");
            InitializeGame();
        }
        else
        {
            Debug.Log("Game not started, subscribing");
            MatchCommunicationManager.Instance.OnGameStarted += InitializeGame;
        }
    }

    private void InitializeGame() {
        MatchCommunicationManager.Instance.OnGameStarted -= InitializeGame;
        var players = MatchCommunicationManager.Instance.Players;
        if(players != null)
        {
            foreach (var player in players)
            {
                CreateShip(player);
            }
        }

        Multiplayer.GameElementsManager.Instance.PopulateGameElements();
    }

    private void EndGame(MatchMessageGameEnded message)
    {
    }

    public void CreateShip(IUserPresence owner) {
        Debug.Log("Creating Ship for User: " + owner.Username);
        var shipGO = Instantiate(_shipPrefab);
        var network = shipGO.GetComponent<NetworkedObject>();
        network.owner = owner;
        
        _networkedGameObjects.Add(owner.UserId, shipGO);
    }

    public GameObject GetObjectWithNetworkId(string id) {
        return _networkedGameObjects[id];
    }

    protected override void OnDestroy()
    {
        MatchCommunicationManager.Instance.OnGameEnded -= EndGame;
    }
}