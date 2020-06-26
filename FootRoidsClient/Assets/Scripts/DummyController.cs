using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyController : MonoBehaviour
{
    public int id;

    public void updatePosition(float x, float y, float z) {
        //temp vector for new position
        var newPos = new Vector3(x, y, z);

        // Set the position back to the transform
        transform.position = newPos;
    }

    public void updateRotation(float x, float y, float z)
    {
        // temp vector 
        var newRot = new Vector3(x, y, z);

        // set the rotaton
        transform.Rotate(x, y, z, Space.Self);
    }  
    
}
