using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Multiplayer;
using Nakama;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [SerializeField] private GameObject _shipPrefab;
    private Dictionary<string, GameObject> _networkedGameObjects = new Dictionary<string, GameObject>();

    private void Awake() {
        MatchCommunicationManager.Instance.OnGameStarted += InitializeGame;
        MatchCommunicationManager.Instance.OnGameEnded += EndGame;
    }

    private void InitializeGame() {
        MatchCommunicationManager.Instance.OnGameStarted -= InitializeGame;
        var players = MatchCommunicationManager.Instance.Players;
        foreach (var player in players) {
            CreateShip(player);
        }
    }

    private void EndGame(MatchMessageGameEnded message) {
        MatchCommunicationManager.Instance.OnGameEnded -= EndGame;
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
}