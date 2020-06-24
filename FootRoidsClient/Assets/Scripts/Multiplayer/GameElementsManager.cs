using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    // used for managing asteroids and other non-player game elements
    public class GameElementsManager : Singleton<GameElementsManager>
    {
        [SerializeField] private GameObject asteroidLargePrefab;
        [SerializeField] private GameObject asteroidMedium1Prefab;
        [SerializeField] private GameObject asteroidMedium2Prefab;
        [SerializeField] private GameObject asteroidSmallPrefab;
        [SerializeField] private GameObject goalPrefab;

        public void PopulateGameElements()
        {
            if(MatchCommunicationManager.Instance.IsHost == false)
            {
                // only the host creates game elements
                return;
            }

            // how many asteroids? for now just hardcode it
            for(int i = 0; i < 2; i++)
            {
                //MatchCommunicationManager.Instance.S
            }
        }                
    }
}
