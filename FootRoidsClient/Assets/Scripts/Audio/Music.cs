using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Music : MonoBehaviour
{
	static Music instance = null;

	private void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
        }
		else
		{
            // Allow audio to keep playing between scenes
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
    }

	public void ToggleSound()
	{
		if (PlayerPrefs.GetInt("Muted", 0) == 0)
		{
            // Turn music on
			PlayerPrefs.SetInt("Muted", 1);
		}
		else
		{
			// Turn music off
			PlayerPrefs.SetInt("Muted", 0);
		}
    }
}
