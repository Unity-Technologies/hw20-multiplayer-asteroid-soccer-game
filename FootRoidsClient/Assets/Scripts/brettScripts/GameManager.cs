using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Multiplayer;
using Nakama;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [SerializeField] private GameObject _shipPrefab;
    private Dictionary<string, GameObject> _networkedGameObjects = new Dictionary<string, GameObject>();    

    void Start()
    {
        if(MatchMaker.Instance.IsHost)
        {
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.StadiumEntered,
                new MatchMessageStadiumEntered(ServerSessionManager.Instance.Session.UserId));
        }
        else
        {
            MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.StadiumEntered,
                new MatchMessageStadiumEntered(ServerSessionManager.Instance.Session.UserId));
        }
    }

    public void InitializeGame() {       
        
        GameSceneController.Instance.PopulateGameElements();
    }
}