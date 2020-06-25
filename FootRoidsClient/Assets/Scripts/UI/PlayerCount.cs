using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCount : MonoBehaviour {

    [SerializeField] private Text _playerCountText = null;
    private int _currentPlayers;
    private int _totalPlayers;

    public void UpdateCurrentPlayers(int newPlayerCount) {
        _currentPlayers = newPlayerCount;
        UpdatePlayerCountText();
    }

    public void UpdateTotalPlayers(int totalPlayerCount) {
        _totalPlayers = totalPlayerCount;
        UpdatePlayerCountText();
    }

    private void UpdatePlayerCountText() {
        _playerCountText.text = $"{_currentPlayers} / {_totalPlayers}";
    }
}
