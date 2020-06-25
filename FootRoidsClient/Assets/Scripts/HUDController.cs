using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Text scoreText;
    public Text pauseButtonText;

    public void PauseButtonClicked()
    {
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
            pauseButtonText.text = "Resume";
        }
        else
        {
            Time.timeScale = 1;
            pauseButtonText.text = "Pause";
        }
    }

}
