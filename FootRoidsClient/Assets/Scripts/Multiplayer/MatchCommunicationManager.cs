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

        public delegate void OnStadiumEnteredHandler();
        private event OnStadiumEnteredHandler OnStadiumEntered;

        public void SubscribeToStadiumEnteredEvent(OnStadiumEnteredHandler stadiumEnteredHandler)
        {
            OnStadiumEntered += stadiumEnteredHandler;
        }

        public void UnsubscribeFromStadiumEnteredEvent(OnStadiumEnteredHandler stadiumEnteredHandler)
        {
            OnStadiumEntered -= stadiumEnteredHandler;
        }

        public event Action OnGameStarted;
        public event Action<MatchMessageGameEnded> OnGameEnded;
        public event Action<MatchMessageAsteroidSpawned> OnAsteroidSpawned;        
        public event Action<Vector3, int> OnPositionUpdated;
        public event Action<float, int> OnRotationUpdated;

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

        public bool AllPlayersJoined {  get { return Players.Count == playersInMatch;  } }

        public bool GameStarted { private set; get; }

        private ISocket _socket { get { return ServerSessionManager.Instance.Socket;  } }

        private int playersInMatch;
        private Queue<IncommingMessageState> inboundMessages = new Queue<IncommingMessageState>();

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            OnGameEnded += GameEnded;
            _socket.ReceivedMatchState += ReceiveMatchStateMessage;
        }

        private void StartGame()
        {
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

        protected override void OnDestroy()
        {
            inboundMessages = new Queue<IncommingMessageState>();
            OnGameEnded -= GameEnded;
        }

        // send messages to server
        public void SendMatchStateMessage<T>(MatchMessageType opCode, T message)
            where T : MatchMessage<T>
        {
            try
            {
                Debug.LogError("Sending Match Data...");
                //Packing MatchMessage object to json
                string json = MatchMessage<T>.ToJson(message);

                //Sending match state json along with opCode needed for unpacking message to server.
                //Then server sends it to other players
                _socket.SendMatchStateAsync(MatchMaker.Instance.GetMatchId(), (long)opCode, json);
            }
            catch (Exception e)
            {
                Debug.LogError("Error while sending match state: " + e.Message);
            }
        }

        // this is  for _host only_ to send messages to themself
        public void SendMatchStateMessageSelf<T>(MatchMessageType opCode, T message)
            where T : MatchMessage<T>
        {
            Debug.LogError("Sending Match Data to Self");
            // TODO: add more cases
            switch(opCode)
            {
                case MatchMessageType.AsteroidSpawned:
                    OnAsteroidSpawned?.Invoke(message as MatchMessageAsteroidSpawned);
                    break;
                case MatchMessageType.StadiumEntered:
                    OnStadiumEntered?.Invoke();
                    break;
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

                    if (AllPlayersJoined)
                    {
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
                case MatchMessageType.AsteroidSpawned:
                    MatchMessageAsteroidSpawned asteroidSpawned = MatchMessageAsteroidSpawned.Parse(messageJson);
                    OnAsteroidSpawned?.Invoke(asteroidSpawned);
                    break;
                case MatchMessageType.StadiumEntered:
                    OnStadiumEntered?.Invoke();
                case MatchMessageType.PositionUpdated:
                    
                    var positionValues = messageJson.FromJson<MatchMessagePositionUpdated>();
                    var pos = new Vector3(positionValues.posX, positionValues.posY, 0.0f);
                    
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnPositionUpdated?.Invoke(pos, 0);
                    });

                    break;
                case MatchMessageType.RotationUpdated:
                    
                    var rotationValues = messageJson.FromJson<MatchMessageRotationUpdated>();
                    
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnRotationUpdated?.Invoke(rotationValues.rot, 0);
                    });

                    break;
                default:
                    Debug.Log("Needs more implementation!");
                    break;
            }
        }        
    }
}

