using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Multiplayer;
using UnityEngine;
using Nakama;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject LoadingPanel;
    public GameObject UsernamePanel;
    public GameObject MatchmakingPanel;
    public GameObject AcceptMatchPanel;
    public PlayerCount ReadyPlayersCount;
    public UnityEngine.UI.Text UsernameInputText;
    public UnityEngine.UI.Text UsernameDisplayText;

    public async void ButtonPressPlay() {
        // show loading
        LoadingPanel.SetActive(true);
        gameObject.SetActive(false);

        await checkDefaultLogin();

        LoadingPanel.SetActive(false);
        if (ServerSessionManager.Instance.Session != null) {
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
        ServerSessionManager.Instance.Session = await startSession(UsernameInputText.text);

        // go to next panel
        showMatchmakingPanel();
    }

    public void ButtonPressMatchmakingStart() {
        // show loading
        LoadingPanel.SetActive(true);
        MatchmakingPanel.SetActive(false);

        MatchMaker.Instance.SubscribeToMatchFoundEvent(OnMatchmakerMatchFound);
        MatchMaker.Instance.StartMatchmaking("*");
    }
    
    public void ButtonPressAcceptMatch() {
        ReadyPlayersCount.gameObject.SetActive(true);
        MatchMaker.Instance.AcceptPendingMatch();
    }

    public void ButtonPressDeclineMatch() {
        
    }

    void showMatchmakingPanel() {
        UsernameDisplayText.text = ServerSessionManager.Instance.Session.Username;
        LoadingPanel.SetActive(false);
        MatchmakingPanel.SetActive(true);
    }

    async Task checkDefaultLogin() {
        var client = ServerSessionManager.Instance.Client;
        try
        {
            ServerSessionManager.Instance.Session = await client.AuthenticateDeviceAsync(getDeviceId(), null, false);
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
        return ServerSessionManager.Instance.Client.AuthenticateEmailAsync(username + "@blah.com", "password");
//        return ServerSessionManager.Instance.Client.AuthenticateDeviceAsync(getDeviceId(), username, true);
    }

    string getDeviceId() {

        // Uncomment this for testing
        return System.Guid.NewGuid().ToString();

        var deviceId = PlayerPrefs.GetString("nakama.deviceid");
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = SystemInfo.deviceUniqueIdentifier;
            PlayerPrefs.SetString("nakama.deviceid", deviceId);
        }
        return deviceId;
    }

    void OnMatchmakerMatchFound(IMatchmakerMatched matched) {
        MatchMaker.Instance.UnsubscribeFromMatchFoundEvent(OnMatchmakerMatchFound);
        MatchMaker.Instance.SubscribeToPlayerJoinedEvent(OnPlayerJoinedMatch);
        MatchMaker.Instance.SubscribeToMatchStartEvent(OnMatchStart);
        
        ReadyPlayersCount.UpdateTotalPlayers(matched.Users.Count());
        AcceptMatchPanel.SetActive(true);
        LoadingPanel.SetActive(false);
    }

    void OnPlayerJoinedMatch(int totalReadyPlayers, IUserPresence newPlayer) {
        ReadyPlayersCount.UpdateCurrentPlayers(totalReadyPlayers);
    }

    void OnMatchStart() {
        MatchMaker.Instance.UnsubscribeFromPlayerJoinedEvent(OnPlayerJoinedMatch);
        MatchMaker.Instance.UnsubscribeFromMatchStartEvent(OnMatchStart);

        gameObject.SetActive(true);
        StartCoroutine(LoadStadium());
    }
    
    IEnumerator LoadStadium() {
        var asyncLoad = SceneManager.LoadSceneAsync("Stadium", LoadSceneMode.Additive);
        while (!asyncLoad.isDone) {
            yield return null;
        }
        SceneManager.UnloadSceneAsync("MainMenu");
    }
}
