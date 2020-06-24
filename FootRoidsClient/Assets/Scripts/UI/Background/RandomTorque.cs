using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTorque : MonoBehaviour
{
    public float MinTorque = 1.0f;
    public float MaxTorque = 10.0f;

	public void Start() {
        var randomTorque = Random.Range(MinTorque, MaxTorque);
        GetComponent<Rigidbody2D>().AddTorque(randomTorque);
	}

}
