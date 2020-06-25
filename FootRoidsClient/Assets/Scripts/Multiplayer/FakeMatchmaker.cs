using System;
using System.Collections;
using System.Collections.Generic;
using Multiplayer;
using UnityEngine;

using Nakama;
using Nakama.TinyJson;
using UnityEngine.SceneManagement;

namespace FootRoids
{
    public class FakeMatchmaker : MonoBehaviour
    {
        IMatch m_Match;
        [SerializeField] private string m_email = "one@one.com";

        async void Start() {
            var client = ServerSessionManager.Instance.Client;
            ServerSessionManager.Instance.Session = await client.AuthenticateEmailAsync(m_email, "password");
            ServerSessionManager.Instance.Socket.ReceivedMatchmakerMatched += MatchmakerMatched;

            await ServerSessionManager.Instance.Socket.ConnectAsync(ServerSessionManager.Instance.Session);
            await ServerSessionManager.Instance.Socket.AddMatchmakerAsync("*", 2, 2);
        }

        void MatchmakerMatched(IMatchmakerMatched matched)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ISocket socket = ServerSessionManager.Instance.Socket;
                socket.ReceivedMatchmakerMatched -= MatchmakerMatched;

                //StartCoroutine(LoadStadium(matched));
            });
        }
        
        IEnumerator LoadStadium(IMatchmakerMatched matched)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Stadium", LoadSceneMode.Additive);
         
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
         
            SceneManager.UnloadSceneAsync("FakeMatchmaker");
            //MatchCommunicationManager.Instance.JoinMatchAsync(matched);
        }

        private IEnumerator LoadGame(IMatchmakerMatched matched)
        {
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MatchScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("FakeMatchmaker");
            //MatchCommunicationManager.Instance.JoinMatchAsync(matched);
        }
    }
}
