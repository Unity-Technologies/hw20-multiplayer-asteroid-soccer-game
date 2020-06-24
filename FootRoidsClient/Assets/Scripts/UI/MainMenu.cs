using System;
using System.Threading.Tasks;
using UnityEngine;
using Nakama;

public class MainMenu : MonoBehaviour
{
    public GameObject LoadingPanel;
    public GameObject UsernamePanel;
    public GameObject MatchmakingPanel;
    public UnityEngine.UI.Text UsernameInputText;
    public UnityEngine.UI.Text UsernameDisplayText;

    // TODO: move all the Nakama stuff somewhere else
    private readonly IClient _client = new Client("http", "127.0.0.1", 7350, "defaultkey");
    private ISession _session;
    private ISocket _socket;
    private IMatch _match;

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
            Debug.LogError("Counldn't connect to Nakama server; message: " + e);
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
        _socket = _client.NewSocket();
        _socket.Connected += () => {
            Debug.Log("Socket Connected!");
        };
        _socket.Closed += () => {
            Debug.Log("Socket Closed!");
        };
        _socket.ReceivedChannelMessage += (message) => {
            Debug.Log("Message Received: " + message);
        };
        _socket.ReceivedMatchmakerMatched += async (matched) => {
            Debug.Log("Matched!");
            var match = await _socket.JoinMatchAsync(matched);
            if (_match == null)
            {
                _match = match;
            }

            // TODO: update some UI

            foreach (var presence in _match.Presences) {
                Debug.Log("presence: "+presence.Username);
            }
        };

        await _socket.ConnectAsync(_session);
        await _socket.AddMatchmakerAsync("*", 2, 2);
    }

    private void OnApplicationQuit()
    {
        _socket?.CloseAsync();
    }
}
