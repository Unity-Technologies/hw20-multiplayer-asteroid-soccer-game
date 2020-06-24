using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    public GameObject PrefabToSpawn;
    public int MinToSpawn = 1;
    public int MaxToSpawn = 10;

    public Rect SpawnBounds;

    void Start() {
        var numToSpawn = Random.Range(MinToSpawn, MaxToSpawn);
        for (var i = 0; i < numToSpawn; i++) {
            spawnPrefab();
        }
    }

    void spawnPrefab() {
        var x = Random.Range(SpawnBounds.xMin, SpawnBounds.xMax);
        var y = Random.Range(SpawnBounds.yMin, SpawnBounds.yMax);
        var pos = new Vector3(x, y, 1);
        Instantiate(PrefabToSpawn, pos, Quaternion.identity);
    }
}
