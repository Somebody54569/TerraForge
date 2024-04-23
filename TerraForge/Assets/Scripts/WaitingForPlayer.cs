using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerlist;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private GameObject waitingPanel;

    private NetworkVariable<bool> IsGameStarted = new NetworkVariable<bool>();

    public GridBuildingSystem _gridBuildingSystem;

    private Queue<Transform> SpawnPoint;

    public bool isDone;
    // Start is called before the first frame update
    void Start()
    {
        SpawnPoint = new Queue<Transform>();
        _gridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
        if (!IsHost)
        {
            startButton.gameObject.SetActive(false);
            waitingText.text = "Wait for Host To Start";
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
                if (IsHost)
                {
                    waitingText.text = "Press To start";
                }
                startButton.interactable = true;
            }
        }
        CheckGameDone();
        if (isDone)
        {
            foreach (var player in playerlist)
            {
                if (player.GetComponent<PlayerManager>().IsLose && player.GetComponent<PlayerManager>().OwnerClientId == this.OwnerClientId)
                {
                    waitingPanel.SetActive(true);
                    waitingText.text = "You are Defeat";
                }
                else
                {
                    waitingPanel.SetActive(true);
                    waitingText.text = "You are Victory";
                }
              
            }
        }
    }

    public void CheckGameDone()
    {
        foreach (var player in playerlist)
        {
            if (player.GetComponent<PlayerManager>().IsLose)
            {
                isDone = true;
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
    
        setSpawnPointServerRpc();
    }

    private void setQspawnPoint()
    {
        foreach (var spawn in _gridBuildingSystem.SpawnPoint)
        {
            SpawnPoint.Enqueue(spawn.transform);
        }
    }
    
    [ServerRpc]
    private void setSpawnPointServerRpc()
    {
            setQspawnPoint();
            foreach (var player in playerlist)
            {
                player.transform.position = SpawnPoint.Dequeue().transform.position;
                player.GetComponent<PlayerManager>().PlayerSpawnWithBase(player.transform.position);
            }
            setSpawnPointClientRpc();
    }
    
    [ClientRpc]
    private void setSpawnPointClientRpc()
    {
        if (IsHost)
        {
            return;
        }
        setQspawnPoint();
        foreach (var player in playerlist)
        {
            player.transform.position = SpawnPoint.Dequeue().transform.position;
            player.GetComponent<PlayerManager>().PlayerSpawnWithBase(player.transform.position);
        }

    }
    

}
