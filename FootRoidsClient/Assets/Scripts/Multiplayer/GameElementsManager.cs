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

        private void Start()
        {
            MatchCommunicationManager.Instance.OnAsteroidSpawned += SpawnAsteroid;
        }

        public void PopulateGameElements()
        {
            Debug.Log("Populating Game Elements");

            if(MatchCommunicationManager.Instance.IsHost == false)
            {
                Debug.LogError("NOT HOST");
                // only the host creates game elements
                return;
            }

            Debug.LogError("IS HOST");

            int ids = 0;
            // how many asteroids? for now just hardcode it
            for(int i = 0; i < 1; i++)
            {                
                MatchMessageAsteroidSpawned roid =
                    new MatchMessageAsteroidSpawned(
                        new Vector3(0, 0, 0), new Vector3(0, 1, 0), ids++);

                // tell the clients
                MatchCommunicationManager.Instance.SendMatchStateMessage(
                    MatchMessageType.AsteroidSpawned, roid);

                // tell yourself (host)
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(
                    MatchMessageType.AsteroidSpawned, roid);
            }
        }

        private void SpawnAsteroid(MatchMessageAsteroidSpawned message)
        {
            Debug.Log("Spawning asteroid");

            // TODO: more details on setting orientation...can we send a transform?
            GameObject roid = Instantiate(asteroidLargePrefab, message.ElementPosition, Quaternion.identity, transform);

            // TODO: handle destruction
            // TODO: need to keep track in a list?            
        }

        protected override void OnDestroy()
        {
            MatchCommunicationManager.Instance.OnAsteroidSpawned -= SpawnAsteroid;
        }
    }
}
