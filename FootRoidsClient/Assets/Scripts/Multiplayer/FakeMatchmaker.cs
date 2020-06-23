using System;
using System.Collections.Generic;
using Multiplayer;
using UnityEngine;

using Nakama;
using Nakama.TinyJson;

namespace FootRoids
{
    [Serializable]
    public struct FakeUser
    {
        public string email;
        public string password;
    }

    public class FakeMatchmaker : MonoBehaviour
    {
        [SerializeField] FakeUser[] m_Users = null;
        readonly IClient m_Client = new Client("http", "207.254.17.33", 7350, "defaultkey");
        List<ISocket> m_Sockets = new List<ISocket>();
        IMatch m_Match;

        async void Start()
        {
            foreach (var user in m_Users)
            {
                ServerSessionManager.Instance.Session = await m_Client.AuthenticateEmailAsync(user.email, user.password);
                ServerSessionManager.Instance.Socket = m_Client.NewSocket();

                ServerSessionManager.Instance.Socket.Connected += () => { Debug.Log("Socket Connected!"); };

                ServerSessionManager.Instance.Socket.Closed += () => { Debug.Log("Socket Closed!"); };

                ServerSessionManager.Instance.Socket.ReceivedChannelMessage += (message) => { Debug.Log("Message Received: " + message); };

                ServerSessionManager.Instance.Socket.ReceivedMatchmakerMatched += async (matched) =>
                {
                    Debug.Log("Matched!");
                    var match = await ServerSessionManager.Instance.Socket.JoinMatchAsync(matched);
                    if (m_Match == null)
                    {
                        m_Match = match;
                    }
                    
                    
                    /*UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        ISocket socket = NakamaSessionManager.Instance.Socket;
                        socket.ReceivedMatchmakerMatched -= OnMatchmakerMatched;

                        StartCoroutine(LoadBattle(e));
                    });*/
                    
                    
                    
                    
                };

                ServerSessionManager.Instance.Socket.ReceivedMatchState += async (state) =>
                {
                    Debug.Log("Received Message From Server: " + state.State);
                };

                await ServerSessionManager.Instance.Socket.ConnectAsync(ServerSessionManager.Instance.Session);
                await ServerSessionManager.Instance.Socket.AddMatchmakerAsync("*", 2, 2);
                m_Sockets.Add(ServerSessionManager.Instance.Socket);
            }
        }

        void OnApplicationQuit()
        {
            foreach (var socket in m_Sockets)
            {
                socket?.CloseAsync();
            }
        }

        void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (m_Match == null)
                {
                    Debug.Log("Match is null");
                }

                Debug.Log("Sending Message To Server!");
                var socket = m_Sockets[0];
                var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
                socket.SendMatchStateAsync(m_Match.Id, 1, newState);
            }
        }
    }
}
