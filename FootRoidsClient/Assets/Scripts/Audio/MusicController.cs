using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    private Music music;
    public Button musicToggleButton;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private void Start()
    {
        music = FindObjectOfType<Music>();
        UpdateIconAndSound();
    }

    public void StopMusic()
    {
        music.ToggleSound();
        UpdateIconAndSound();
    }

    private void UpdateIconAndSound()
    {
        if (PlayerPrefs.GetInt("Muted", 0) == 0)
        {
            AudioListener.volume = 1;
            musicToggleButton.GetComponent<Image>().sprite = musicOnSprite;
        }
        else
        {
            AudioListener.volume = 0;
            musicToggleButton.GetComponent<Image>().sprite = musicOffSprite;
        }
    }
}
