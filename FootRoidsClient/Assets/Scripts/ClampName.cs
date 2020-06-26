using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClampName : MonoBehaviour
{
    public Text nameLabel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Takes world point and converts to screen coordinates
        // looks for camera with tag of main camera
        Vector3 namePos = Camera.main.WorldToScreenPoint(this.transform.position);
        //setting text transform position to equal our namePosition, which should be 2d coordinate
        nameLabel.transform.position = namePos;
    }
}
