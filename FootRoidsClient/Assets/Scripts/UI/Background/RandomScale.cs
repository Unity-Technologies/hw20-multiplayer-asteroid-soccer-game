using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScale : MonoBehaviour
{
    public float MinScale = 1.0f;
    public float MaxScale = 2.0f;

    void Start() {
        transform.localScale = transform.localScale * Random.Range(MinScale, MaxScale);
    }
}
