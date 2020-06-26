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

        // TODO (matt): We probably don't need these
        public event Action OnGameStarted;
        public event Action<MatchMessageGameEnded> OnGameEnded;
        // end

        public event Action<MatchMessageSpawnElement> OnAsteroidSpawned;
        public event Action<MatchMessageSpawnElement> OnPlayerSpawned;
        public event Action<MatchMessageSpawnElement> OnBallSpawned;
        public event Action<MatchMessageSpawnElement> OnGoalSpawned;

        public event Action<float, float, float, int> OnPlayerPositionUpdated;
        public event Action<float, int> OnPlayerInputRotationUpdated;
        public event Action<float, int> OnPlayerInputThrustUpdated;

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
                //Debug.LogError("Sending Match Data...");
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
                    OnAsteroidSpawned?.Invoke(message as MatchMessageSpawnElement);
                    break;
                case MatchMessageType.PlayerSpawned:
                    OnPlayerSpawned?.Invoke(message as MatchMessageSpawnElement);
                    break;
                case MatchMessageType.BallSpawned:
                    OnBallSpawned?.Invoke(message as MatchMessageSpawnElement);
                    break;
                case MatchMessageType.GoalSpawned:
                    OnGoalSpawned?.Invoke(message as MatchMessageSpawnElement);
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
                    MatchMessageSpawnElement asteroidSpawn = MatchMessageSpawnElement.Parse(messageJson);
                    OnAsteroidSpawned?.Invoke(asteroidSpawn);
                    break;
                case MatchMessageType.StadiumEntered:
                    OnStadiumEntered?.Invoke();
                    break;
                case MatchMessageType.PlayerSpawned:
                    MatchMessageSpawnElement playerSpawn = MatchMessageSpawnElement.Parse(messageJson);
                    OnPlayerSpawned?.Invoke(playerSpawn);
                    break;
                case MatchMessageType.BallSpawned:
                    MatchMessageSpawnElement ballSpawn = MatchMessageSpawnElement.Parse(messageJson);
                    OnBallSpawned?.Invoke(ballSpawn);
                    break;
                case MatchMessageType.GoalSpawned:
                    MatchMessageSpawnElement goalSpawn = MatchMessageSpawnElement.Parse(messageJson);
                    OnGoalSpawned?.Invoke(goalSpawn);
                    break;
                case MatchMessageType.PlayerPositionUpdated:
                {
                    var positionValues = messageJson.FromJson<MatchMessagePositionUpdated>();

                    var posX = positionValues.posX;
                    var posY = positionValues.posY;
                    var angle = positionValues.angle;
                    var id = positionValues.id;
                    
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnPlayerPositionUpdated?.Invoke(posX, posY, angle, id);
                    });
                }
                    break;
                case MatchMessageType.PlayerInputRotationUpdated:
                {
                    var value = messageJson.FromJson<MatchMessageInputRotationUpdated>();

                    var input = value.input;
                    var id = value.id;
                    
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnPlayerInputRotationUpdated?.Invoke(input, id);
                    });
                }
                    break;
                case MatchMessageType.PlayerInputThrustUpdated:
                {
                    var value = messageJson.FromJson<MatchMessageInputThrustUpdated>();

                    var input = value.input;
                    var id = value.id;
                    
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnPlayerInputThrustUpdated?.Invoke(input, id);
                    });
                }

                    break;
                default:
                    Debug.Log("Needs more implementation!");
                    break;
            }
        }        
    }
}

