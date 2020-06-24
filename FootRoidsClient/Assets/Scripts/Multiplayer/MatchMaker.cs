using System;
using System.Collections;
using System.Collections.Generic;
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
    private List<IUserPresence> _readyPlayers;
    
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

    public async void StartMatchmaking(string query) {
        ServerSessionManager.Instance.Socket.ReceivedMatchmakerMatched += OnMatchMakerMatched;
        _readyPlayers = new List<IUserPresence>();
        await ServerSessionManager.Instance.Socket.AddMatchmakerAsync(query, _minPlayerCount, _maxPlayerCount);
    }

    void OnMatchMakerMatched(IMatchmakerMatched matched) {
        ServerSessionManager.Instance.Socket.ReceivedMatchmakerMatched -= OnMatchMakerMatched;
        _pendingMatch = matched;

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
}