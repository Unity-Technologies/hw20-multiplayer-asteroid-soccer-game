using System;
using System.Collections.Generic;
using System.Linq;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;

public class NetworkManager : MonoBehaviour {

    [SerializeField] private string email;

    private readonly IClient _client = new Client("http", "127.0.0.1", 7350, "defaultkey");
    private ISocket _socket;
    private IMatch _match;
    private GameManager _gameManager;
    private IUserPresence _self;

    private void Awake() {
        _gameManager = GetComponent<GameManager>();
    }

    private async void Start() {
        var session = await _client.AuthenticateEmailAsync(email, "password");
        var socket = _client.NewSocket();
        socket.Connected += () => {
            Debug.Log("Socket Connected!");
        };
        socket.Closed += () => {
            Debug.Log("Socket Closed!");
        };
        socket.ReceivedChannelMessage += (message) => {
            Debug.Log("Message Received: " + message);
        };
        socket.ReceivedMatchmakerMatched += async (matched) => {
            var match = await socket.JoinMatchAsync(matched);
        };
        socket.ReceivedMatchState += (state) => {
            Debug.Log("Received Message From Server: " + state.State);
        };
        socket.ReceivedMatchPresence += (presence) => {
            foreach (var user in presence.Joins) {
                OnPlayerJoined(user);
            }
            foreach (var user in presence.Leaves) {
                OnPlayerLeft(user);
            }
        };

        await socket.ConnectAsync(session);
        await socket.AddMatchmakerAsync("*", 2, 2);
        _socket= socket;
    }

    private void OnApplicationQuit() {
        _socket?.CloseAsync();
    }
    
    private void OnMatchJoined(IMatch match) {
        _self = match.Self;
        var participants = match.Presences.ToArray();
        foreach (var player in participants) {
//            MainThreadDispatcher.Instance().Enqueue(_gameManager.CreateShip(player));
        }
    }

    private void OnPlayerJoined(IUserPresence player) {
//        MainThreadDispatcher.Instance().Enqueue(_gameManager.CreateShip(player));
    }

    private void OnPlayerLeft(IUserPresence player) {
        //TODO Destroy player's ship object
        Debug.Log("Player Left: " + player.Username);
    }

    private void FixedUpdate() {
        
//        if (Input.GetKeyDown(KeyCode.P) ) {
//            if (_match == null) {
//                Debug.Log("Match is null");
//            }
//            Debug.Log("Sending Message To Server!");
//            var socket = _sockets[0];
//            var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
//            socket.SendMatchStateAsync(_match.Id, 1, newState);
//        }
    }
}