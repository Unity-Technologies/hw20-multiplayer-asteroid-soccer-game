using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Multiplayer;
using Nakama;
using UnityEngine;

public class MatchMaker : Singleton<MatchMaker> {

    [SerializeField] private int _minPlayerCount = 2;
    [SerializeField] private int _maxPlayerCount = 2;

    private OnMatchFoundHandler _matchMadeCallback;
    private IMatchmakerMatched _pendingMatch;
    private IMatch _joinedMatch;
    
    public ReadOnlyCollection<IUserPresence> ReadyPlayers => new ReadOnlyCollection<IUserPresence>(_readyPlayers);
    private List<IUserPresence> _readyPlayers;

    private bool forceHost = true;

#if UNITY_EDITOR
    private string fakeHostId = "it me";
#else
    private string fakeHostId = "it n me";
#endif

    [SerializeField]
    public string CurrentHostId;

    public bool IsHost
    {
        get
        {
            //Debug.LogError("UserID " + ServerSessionManager.Instance.Session.UserId);
            //Debug.LogError("Current Host ID: " + CurrentHostId);
            return (CurrentHostId == ServerSessionManager.Instance.Session.UserId || CurrentHostId == fakeHostId);
        }
    }

    public delegate void OnMatchFoundHandler(IMatchmakerMatched matched);
    private event OnMatchFoundHandler OnMatchFound;
    
    public delegate void OnPlayerJoinedHandler(int totalReadyPlayers, IUserPresence newPlayer);
    private event OnPlayerJoinedHandler OnPlayerJoined;
    
    public delegate void OnMatchStartHandler();
    private event OnMatchStartHandler OnMatchStart;

    public void SubscribeToMatchFoundEvent(OnMatchFoundHandler matchFoundHandler) {
        OnMatchFound += matchFoundHandler;
    }

    public void UnsubscribeFromMatchFoundEvent(OnMatchFoundHandler matchFoundHandler) {
        OnMatchFound -= matchFoundHandler;
    }

    public void SubscribeToPlayerJoinedEvent(OnPlayerJoinedHandler playerJoinedHandler) {
        OnPlayerJoined += playerJoinedHandler;
    }

    public void UnsubscribeFromPlayerJoinedEvent(OnPlayerJoinedHandler playerJoinedHandler) {
        OnPlayerJoined -= playerJoinedHandler;
    }
    
    public void SubscribeToMatchStartEvent(OnMatchStartHandler matchStartHandler) {
        OnMatchStart += matchStartHandler;
    }

    public void UnsubscribeFromMatchStartEvent(OnMatchStartHandler matchStartHandler) {
        OnMatchStart -= matchStartHandler;
    }

    public IMatchmakerMatched GetMatched() {
        return Instance._pendingMatch;
    }

    public string GetMatchId()
    {
        return Instance._joinedMatch.Id;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        MatchCommunicationManager.Instance.SubscribeToStadiumEnteredEvent(OnStadiumEntered);        
    }

    public async void StartMatchmaking(string query) {
        ServerSessionManager.Instance.Socket.ReceivedMatchmakerMatched += OnMatchMakerMatched;
        _readyPlayers = new List<IUserPresence>();
        await ServerSessionManager.Instance.Socket.AddMatchmakerAsync(query, _minPlayerCount, _maxPlayerCount);
    }

    int count = 0; //hax
    void OnStadiumEntered()
    {
        if(IsHost)
        {
            count++;
            if(count == _pendingMatch.Users.Count())
            {
                // TODO: make more robust by checking userid
                Debug.LogError("In MainScene " + count);
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    FindObjectOfType<GameSceneController>().InitializeGame(); // hax
                });
            }
        }
    }

    void OnMatchMakerMatched(IMatchmakerMatched matched) {
        ServerSessionManager.Instance.Socket.ReceivedMatchmakerMatched -= OnMatchMakerMatched;
        _pendingMatch = matched;

        ChooseHost();        

        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            OnMatchFound?.Invoke(matched);
        });
    }

    public async void AcceptPendingMatch() {
        if (_pendingMatch == null) {
            Debug.LogWarning("Attempted to join non-existent match");
            return;
        }

        ServerSessionManager.Instance.Socket.ReceivedMatchPresence += OnMatchPresence;
        _joinedMatch = await ServerSessionManager.Instance.Socket.JoinMatchAsync(_pendingMatch);

        var alreadyJoinedPlayers = _joinedMatch.Presences.ToArray();
        foreach (var player in alreadyJoinedPlayers) {
            if (!HasPlayerAlreadyJoined(player)) {
                NewPlayerJoined(player);
            }
        }

    }

    private void OnMatchPresence(IMatchPresenceEvent presenceEvent) {
        foreach (var player in presenceEvent.Joins) {
            if (!HasPlayerAlreadyJoined(player)) {
                NewPlayerJoined(player);
            }
        }
    }

    private void StartMatch() {
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            OnMatchStart?.Invoke();
        });
    }

    private void NewPlayerJoined(IUserPresence player) {
        _readyPlayers.Add(player);
        
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            OnPlayerJoined?.Invoke(_readyPlayers.Count, player);
        });
        if (_readyPlayers.Count >= _pendingMatch.Users.Count()) {
            StartMatch();
        }
    }

    private bool HasPlayerAlreadyJoined(IUserPresence player) {
        return _readyPlayers.Contains(player);
    }

    private void ChooseHost()
    {       
        if(forceHost)
        {
            if(fakeHostId == "it me")
            {
                CurrentHostId = fakeHostId;
            }

            return;
        }

        // Add the session id of all users connected to the match
        List<string> userSessionIds = new List<string>();
        foreach (IMatchmakerUser user in _pendingMatch.Users)
        {
            userSessionIds.Add(user.Presence.SessionId);
        }

        // Perform a lexicographical sort on list of user session ids
        userSessionIds.Sort();

        // First user from the sorted list will be the host of current match
        string hostSessionId = userSessionIds.First();

        // Get the user id from session id
        IMatchmakerUser hostUser = _pendingMatch.Users.First(x => x.Presence.SessionId == hostSessionId);
        CurrentHostId = hostUser.Presence.UserId;
        Debug.Log("HOST ID: " + CurrentHostId);
    }
}