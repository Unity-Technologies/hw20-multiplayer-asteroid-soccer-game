using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomForce : MonoBehaviour
{
    public float MinForce = 1.0f;
    public float MaxForce = 10.0f;

	public void Start() {
        var randomForce = Random.Range(MinForce, MaxForce);
        GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle * randomForce);
	}

}
