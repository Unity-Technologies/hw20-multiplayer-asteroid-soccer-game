using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    [Header("Player Settings")]
    [Range(5, 20)]
    public float playerSpeed;

    [Header("Screen Settings")]
    [Space]
    public Vector3 screenBounds;

    [Header("Screen Dimensions")]
    [Space]
    public float screenTop;
    public float screenBottom;
    public float screenRight;
    public float screenLeft;

    [Header("IsMaster")]
    public bool isMaster;

    private HUDController hUDController;
    private int totalPoints;
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        hUDController = FindObjectOfType<HUDController>();
        screenBounds = GetScreenBounds();
        player = FindObjectOfType<PlayerController>();
        // Spawn asteroids in random locations
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Get the screen bounds
    private Vector3 GetScreenBounds()
    {
        Camera mainCamera = Camera.main;
        Vector3 screenVector = new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z);

        Debug.Log($"ScreenToWorldPoint is: {mainCamera.ScreenToWorldPoint(screenVector)}");

        return mainCamera.ScreenToWorldPoint(screenVector);
    }
}
