using System;
using UnityEngine;

using Nakama;

namespace Multiplayer
{
    public class ServerSessionManager : Singleton<ServerSessionManager>
    {
//        [SerializeField] string _serverAddress = "207.254.17.33";
        [SerializeField] string _serverAddress = "34.69.157.238";
        [SerializeField] int _port = 7350;
        
        string _deviceId;

        /// <summary>
        /// Used to establish connection between the client and the server.
        /// Contains a list of usefull methods required to communicate with Nakama server.
        /// Do not use this directly, use <see cref="Client"/> instead.
        /// </summary>
        Client _client;

        /// <summary>
        /// Socket responsible for maintaining connection with Nakama server and exchanger realtime messages.
        /// Do not use this directly, use <see cref="Socket"/> instead.
        /// </summary>
        ISocket _socket;

        /// <summary>
        /// Used to communicate with Nakama server.
        /// For the user to send and receive messages from the server, <see cref="Session"/> must not be expired.
        /// Default expiration time is 60s, but for this demo we set it to 3 weeks (1 814 400 seconds).
        /// To initialize the session, call <see cref="AuthenticateDeviceIdAsync"/> or <see cref="AuthenticateFacebookAsync"/> methods.
        /// To reinitialize expired session, call <see cref="Reauthenticate"/> method.
        /// </summary>
        private ISession _session;
        public ISession Session {
            get { return _session; }
            set {
                if (_session != null) {
                    Debug.LogWarning("Starting Multiple Sessions on same Device!");
                }
                _session = value;
                Socket.ConnectAsync(_session);
            } 
        }
        
        /// <summary>
        /// Contains all the identifying data of a <see cref="Client"/>, like User Id, linked Device IDs,
        /// linked Facebook account, username, etc.
        /// </summary>
        public IApiAccount Account { get; set; }

        /// <summary>
        /// Used to establish connection between the client and the server.
        /// Contains a list of usefull methods required to communicate with Nakama server.
        /// </summary>
        public Client Client => _client ?? (_client = new Client("http", _serverAddress, _port, "defaultkey", UnityWebRequestAdapter.Instance));

        /// <summary>
        /// Socket responsible for maintaining connection with Nakama server and exchange realtime messages.
        /// </summary>
        public ISocket Socket
        {
            get
            {
                if (_socket == null)
                {
                    // Initializing socket
                    _socket = _client.NewSocket();
                }
                
                return _socket;
            }

            set => _socket = value;
        }

        public bool IsConnected
        {
            get
            {
                if (Session == null || Session.HasExpired(DateTime.UtcNow) == true)
                {
                    return false;
                }

                return true;
            }
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            GetDeviceId();
        }

        public void SetIp(string ip)
        {
            if (IsConnected == false)
            {
                _serverAddress = ip;
            }
        }

        /// <summary>
        /// Removes session and account from cache, logs out of Facebook and invokes <see cref="OnDisconnected"/>.
        /// </summary>
        public void Disconnect()
        {
            if (Session != null)
            {
                Session = null;
                Account = null;

                Debug.Log("Disconnected from Nakama");
            }
        }

        /// <summary>
        /// Retrieves device id from player prefs. If it's the first time running this app
        /// on this device, <see cref="_deviceId"/> is filled with <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// </summary>
        private void GetDeviceId()
        {
            if (string.IsNullOrEmpty(_deviceId) == true)
            {
                _deviceId = PlayerPrefs.GetString("nakama.deviceId");
                if (string.IsNullOrWhiteSpace(_deviceId) == true)
                {
                    // SystemInfo.deviceUniqueIdentifier is not supported in WebGL,
                    // we generate a random one instead via System.Guid
#if UNITY_WEBGL && !UNITY_EDITOR
                    _deviceId = System.Guid.NewGuid().ToString();
#else
                    _deviceId = SystemInfo.deviceUniqueIdentifier;
#endif                    
                    PlayerPrefs.SetString("nakama.deviceId", _deviceId);
                }
            }
        }
    }
}
