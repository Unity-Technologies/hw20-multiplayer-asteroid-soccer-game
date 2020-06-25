using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Multiplayer;
using Nakama;
using UnityEngine;

public class GameManager : Singleton<GameManager> {

    [SerializeField] private GameObject _shipPrefab;
    private Dictionary<string, GameObject> _networkedGameObjects = new Dictionary<string, GameObject>();
    public string CurrentHostId { private set; get; }

    public bool IsHost
    {
        get
        {
            Debug.LogError("UserID " + ServerSessionManager.Instance.Session.UserId);
            Debug.LogError("Current Host ID: " + CurrentHostId);
            return CurrentHostId == ServerSessionManager.Instance.Session.UserId;
        }
    }

    private void Start()
    {
        
        MatchCommunicationManager.Instance.OnGameEnded += EndGame;

        ChooseHost();
        InitializeGame();

        //if(MatchCommunicationManager.Instance.GameStarted)
        //{
        //    Debug.Log("Game already started, initializing");
        //    InitializeGame();
        //}
        //else
        //{
        //    Debug.Log("Game not started, subscribing");
        //    MatchCommunicationManager.Instance.OnGameStarted += InitializeGame;
        //}
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

    private void ChooseHost()
    {
        var matched = MatchMaker.Instance.GetMatched();

        // Add the session id of all users connected to the match
        List<string> userSessionIds = new List<string>();
        foreach (IMatchmakerUser user in matched.Users)
        {
            userSessionIds.Add(user.Presence.SessionId);
        }

        // Perform a lexicographical sort on list of user session ids
        userSessionIds.Sort();

        // First user from the sorted list will be the host of current match
        string hostSessionId = userSessionIds.First();

        // Get the user id from session id
        IMatchmakerUser hostUser = matched.Users.First(x => x.Presence.SessionId == hostSessionId);
        CurrentHostId = hostUser.Presence.UserId;
        Debug.Log("HOST ID: " + CurrentHostId);
    }
}