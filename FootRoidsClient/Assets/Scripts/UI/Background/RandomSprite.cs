using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSprite : MonoBehaviour
{
    public List<Sprite> Sprites;

    void Start() {
        var index = Random.Range(0, Sprites.Count);
        GetComponent<SpriteRenderer>().sprite = Sprites[index];
    }
}
