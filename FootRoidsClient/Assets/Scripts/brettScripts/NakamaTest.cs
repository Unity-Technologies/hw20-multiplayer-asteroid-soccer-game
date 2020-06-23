using System;
using System.Collections.Generic;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;

[Serializable]
public struct User {
    public string email;
    public string password;
}

public class NakamaTest : MonoBehaviour {

    [SerializeField] private User[] _users = null;
    private readonly IClient _client = new Client("http", "127.0.0.1", 7350, "defaultkey");
    private List<ISocket> _sockets = new List<ISocket>();
    private IMatch _match;

    private async void Start() {
        foreach (var user in _users) {
            var session = await _client.AuthenticateEmailAsync(user.email, user.password);
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
                Debug.Log("Matched!");
                var match = await socket.JoinMatchAsync(matched);
                if (_match == null) {
                    _match = match;
                }
            };
            socket.ReceivedMatchState += async (state) => {
                Debug.Log("Received Message From Server: " + state.State);
            };

            await socket.ConnectAsync(session);
            await socket.AddMatchmakerAsync("*", 2, 2);
            _sockets.Add(socket);
        }
    }

    private void OnApplicationQuit() {
        foreach (var socket in _sockets) {
            socket?.CloseAsync();
        }
    }

    private void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.P) ) {
            if (_match == null) {
                Debug.Log("Match is null");
            }
            Debug.Log("Sending Message To Server!");
            var socket = _sockets[0];
            var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
            socket.SendMatchStateAsync(_match.Id, 1, newState);
        }
    }
}