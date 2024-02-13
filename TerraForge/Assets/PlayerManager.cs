using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    /*
    private GridBuildingSystem _gridBuildingSystem;
    

    private void Start()
    {
        _gridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
    }
    
    public void InitializeWithBuilding(GameObject building)
    {
        _gridBuildingSystem.temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        _gridBuildingSystem.FollowBuilding();
        
        // Find the player manager associated with the provided player NetworkId
        PlayerManager playerManager = FindPlayerManagerByNetworkId(this.OwnerClientId);
        if (playerManager != null)
        {
            _gridBuildingSystem.temp._PlayerManager = playerManager;
        }
    }
    
    [ServerRpc]
    public void TakeAreaServerRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area)
    {
        _gridBuildingSystem.TakeArea(area);
        TakeAreaClientRpc(area);
    }
    
    [ClientRpc]
    private void TakeAreaClientRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area)
    {
        if (IsOwner) { return; }
        _gridBuildingSystem.TakeArea(area);
    }

    private PlayerManager FindPlayerManagerByNetworkId(ulong networkId)
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (player.ClientId == networkId)
            {
                return player.PlayerObject.GetComponent<PlayerManager>();
            }
        }
        return null;
    }*/
}