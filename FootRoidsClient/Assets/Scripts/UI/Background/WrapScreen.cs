using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapScreen : MonoBehaviour
{
    public Rect ScreenBounds;

    void Update()
    {
        Vector2 newPos = transform.position;

        if (transform.position.x > ScreenBounds.xMax)
        {
            newPos.x = ScreenBounds.xMin;
        }
        else if (transform.position.x < ScreenBounds.xMin)
        {
            newPos.x = ScreenBounds.xMax;
        }


        if (transform.position.y > ScreenBounds.yMax)
        {
            newPos.y = ScreenBounds.yMin;
        }
        else if (transform.position.y < ScreenBounds.yMin)
        {
            newPos.y = ScreenBounds.yMax;
        }

        transform.position = newPos;
    }
}
