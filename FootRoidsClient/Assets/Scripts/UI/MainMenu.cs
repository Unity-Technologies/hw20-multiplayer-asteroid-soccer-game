using System;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;
using System.Collections.Generic;
using Nakama.TinyJson;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject LoadingPanel;
    public GameObject UsernamePanel;
    public GameObject MatchmakingPanel;
    public UnityEngine.UI.Text UsernameInputText;
    public UnityEngine.UI.Text UsernameDisplayText;
    public UnityEngine.UI.Text ChatroomText; // TODO

    // TODO: move all the Nakama stuff somewhere else
    private readonly IClient _client = new Client("http", "35.188.32.254", 7350, "defaultkey");
    private ISession _session;
    private ISocket _socket;
    private IMatch _match;
    private const string RoomName = "Cool Chat";

    public async void ButtonPressPlay() {
        // show loading
        LoadingPanel.SetActive(true);
        gameObject.SetActive(false);

        await checkDefaultLogin();

        LoadingPanel.SetActive(false);
        if (_session != null) {
            showMatchmakingPanel();
        } else {
            UsernamePanel.SetActive(true);
        }
    }

    public async void ButtonPressUsernameAccept() {
        // show loading
        LoadingPanel.SetActive(true);
        UsernamePanel.SetActive(false);

        // start multiplayer session
        Debug.Log("ButtonPressUsernameAccept: session? ");
        Debug.Log(_session);
        _session = await startSession(UsernameInputText.text);

        // go to next panel
        showMatchmakingPanel();
    }

    public void ButtonPressMatchmakingStart() {
        // show loading
        LoadingPanel.SetActive(true);
        MatchmakingPanel.SetActive(false);

        startMatchmaking();
    }

    void showMatchmakingPanel() {
        UsernameDisplayText.text = _session.Username;
        LoadingPanel.SetActive(false);
        MatchmakingPanel.SetActive(true);
    }


    // TODO: move all the Nakama stuff somewhere else
    async Task checkDefaultLogin() {
        try
        {
            _session = await _client.AuthenticateDeviceAsync(getDeviceId(), null, false);
        }
        catch (ApiResponseException e)
        {
            if (e.StatusCode != (long)System.Net.HttpStatusCode.NotFound)
            {
                Debug.LogError("An error has occured reaching Nakama server; message: " + e);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Couldn't connect to Nakama server; message: " + e);
        }
    }

    Task<ISession> startSession(string username)
    {
        // TODO: needs some error handling...
        return _client.AuthenticateDeviceAsync(getDeviceId(), username, true);
    }

    string getDeviceId() {
        var deviceId = PlayerPrefs.GetString("nakama.deviceid");
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = SystemInfo.deviceUniqueIdentifier;
            PlayerPrefs.SetString("nakama.deviceid", deviceId);
        }
        return deviceId;
    }

    async void startMatchmaking() {
        //Socket Handlers
        _socket = _client.NewSocket();
        _socket.Connected += () =>
        {
            Debug.Log("Socket Connected!");
        };
        _socket.Closed += () =>
        {
            Debug.Log("Socket Closed!");
        };
        _socket.ReceivedChannelMessage += (message) =>
        {
            Debug.Log("Message Received: " + message);
        };
        _socket.ReceivedMatchmakerMatched += async (matched) =>
        {
            Debug.Log("Matched!");
            var match = await _socket.JoinMatchAsync(matched);
            Debug.LogFormat("Match? {0}", match);

            var self = match.Self;
            Debug.LogFormat("Self: {0}", self);
            Debug.LogFormat("Presences: {0}", match.Presences);

            if (_match == null)
            {
                _match = match;
            }

            // TODO: update some UI

            foreach (var presence in _match.Presences)
            {
                Debug.Log("presence: " + presence.Username);
            }

            // Transition to Game
            SceneManager.LoadScene("MainGame");


        };

        await _socket.ConnectAsync(_session);
        await _socket.AddMatchmakerAsync("*", 2, 9);

        var channel = await _socket.JoinChatAsync(RoomName, ChannelType.Room);
        Debug.LogFormat("Join chat channel: {0}", channel);

        var content = new Dictionary<string, string> { { "hello", "world" } }.ToJson();
        _ = _socket.WriteChatMessageAsync(channel, content);

        // Fake another user to test matchmaking
        //var deviceId2 = Guid.NewGuid().ToString();
        //var session2 = await _client.AuthenticateDeviceAsync(deviceId2);
        //var socket2 = _client.NewSocket();
        //socket2.ReceivedMatchmakerMatched += async matched => await socket2.JoinMatchAsync(matched);
        //await socket2.ConnectAsync(session2);
        //await socket2.AddMatchmakerAsync("*", 2, 9);
        //await Task.Delay(TimeSpan.FromSeconds(120));
        //await socket2.CloseAsync();
    }

    private void OnApplicationQuit()
    {
        _socket?.CloseAsync();
    }
}
