using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using Nakama.TinyJson;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace Multiplayer
{
    public class MatchCommunicationManager : Singleton<MatchCommunicationManager>
    {
        public event Action OnGameStarted;
        public event Action<MatchMessageGameEnded> OnGameEnded;

        public string CurrentHostId { private set; get; }
        public string MatchId { private set; get; }
        

        public bool IsHost
        {
            get
            {
                return CurrentHostId == ServerSessionManager.Instance.Session.UserId;
            }
        }

        public List<IUserPresence> Players { private set; get; }

        //public bool AllPlayersJoined {  get { return Players.Count == _playerCount;  } }

        public bool GameStarted { private set; get; }

        private ISocket _socket { get { return ServerSessionManager.Instance.Socket;  } }

        private bool allPlayersAdded;
        private bool matchJoined;
        private bool isLeaving;
        private Queue<IncommingMessageState> inboundMessages = new Queue<IncommingMessageState>();

        private void Start()
        {
            OnGameEnded += GameEnded;
        }

        private void StartGame()
        {
            if(GameStarted == true)
            {
                return;
            }
            if(allPlayersAdded == false || matchJoined == false)
            {
                return;
            }
            GameStarted = true;

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("Starting Game!");
                OnGameStarted?.Invoke();
                while(inboundMessages.Count > 0)
                {
                    IncommingMessageState inboundMessage = inboundMessages.Dequeue();
                    ReceiveMatchStateHandle(inboundMessage.opCode, inboundMessage.message);
                }
            });            
        }

        private void GameEnded(MatchMessageGameEnded obj)
        {
            _socket.ReceivedMatchPresence -= OnMatchPresence;
        }

        public async void JoinMatchAsync(IMatchmakerMatched matched)
        {
            ChooseHost(matched);

            Players = new List<IUserPresence>();

            try
            {
                // Listen to incomming match messages and user connection changes
                _socket.ReceivedMatchPresence += OnMatchPresence;
                _socket.ReceivedMatchState += ReceiveMatchStateMessage;

                // Join the match
                IMatch match = await _socket.JoinMatchAsync(matched);
                // Set current match id
                // It will be used to leave the match later
                MatchId = match.Id;
                Debug.Log("Joined match with id: " + match.Id + "; presences count: " + match.Presences.Count());


                bool playersJoin = AddConnectedPlayers(match);
                if(playersJoin)
                {
                    matchJoined = true;
                    StartGame();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't join match: " + e.Message);
            }
        }

        private void OnMatchPresence(IMatchPresenceEvent e)
        {
            foreach(IUserPresence user in e.Joins)
            {
                if (Players.FindIndex(x => x.UserId == user.UserId) == -1)
                {
                    Debug.Log("User " + user.Username + " joined match");
                    Players.Add(user);
                    
                    if (Players.Count == e.Joins.Count<IUserPresence>())
                    {
                        allPlayersAdded = true;
                        StartGame();
                    }
                }
                else
                {
                    Debug.Log("Already found this player");
                }
            }
        }

        private void ReceiveMatchStateMessage(IMatchState matchState)
        {
            string messageJson = System.Text.Encoding.UTF8.GetString(matchState.State);

            if (string.IsNullOrEmpty(messageJson))
            {
                return;
            }

            ReceiveMatchStateHandle(matchState.OpCode, messageJson);
        }

        public void ReceiveMatchStateHandle(long opCode, string messageJson)
        {
            if(GameStarted == false)
            {
                inboundMessages.Enqueue(new IncommingMessageState(opCode, messageJson));
            }

            switch((MatchMessageType)opCode)
            {
                case MatchMessageType.MatchEnded:
                    break;
                default:
                    Debug.Log("Needs more implementation!");
                    break;

            }
        }

        private void ChooseHost(IMatchmakerMatched matched)
        {
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
        }

        private bool AddConnectedPlayers(IMatch match)
        {
            foreach(IUserPresence user in match.Presences)
            {
                if(Players.FindIndex(x => x.UserId == user.UserId) == -1)
                {
                    Debug.Log("User +" + user.Username + " joined match");

                    Players.Add(user);                    

                    // TODO: need to set opponent id?

                    if(Players.Count == match.Presences.Count<IUserPresence>())
                    {
                        allPlayersAdded = true;
                    }
                }
                else
                {
                    Debug.LogError("Not allowed!");
                    return false;
                }
            }

            return true;
        }
    }
}

