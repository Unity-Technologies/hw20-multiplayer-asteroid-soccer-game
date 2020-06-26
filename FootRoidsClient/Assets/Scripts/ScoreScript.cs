using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    Text scoreText;

    void Start() {
        scoreText = GetComponent<Text>();
        SetScore(0);
    }

    public void SetScore(int newScore) {
        scoreText.text = $"Score: {newScore}";
    }
}
