using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerlist;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject waitingPanel;

    private NetworkVariable<bool> IsGameStarted = new NetworkVariable<bool>();

    // Start is called before the first frame update
    void Start()
    {
        if (!IsHost)
        {
            startButton.gameObject.SetActive(false);
        }

        IsGameStarted.OnValueChanged += OnGameStartedChanged;
    }

    private void OnDestroy()
    {
        IsGameStarted.OnValueChanged -= OnGameStartedChanged;
    }

    private void OnGameStartedChanged(bool oldValue, bool newValue)
    {
        if (newValue == true)
        {
            waitingPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsGameStarted.Value)
        {
            playerlist = GameObject.FindGameObjectsWithTag("Player");
            if (playerlist.Length != 2)
            {
                startButton.interactable = false;
            }
            else
            {
                startButton.interactable = true;
            }
        }
    }

    public void StartGame()
    {
        IsGameStarted.Value = true;
        foreach (var player in playerlist)
        {
            player.GetComponent<PlayerManager>().IsPlayerMax.Value = true;
        }
    }
}
