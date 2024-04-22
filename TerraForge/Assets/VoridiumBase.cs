using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VoridiumBase : NetworkBehaviour
{
    private ulong ownerClientId;
    public SpriteRenderer SpriteRenderer;
    public Color unitColor;
    public int RaisRate = 50;
    public PlayerManager _PlayerManager;
    
    public void ChangeOwner(ulong uUlong)
    {
        uUlong = this.OwnerClientId;
    }

    public void ChangeRais()
    {
        _PlayerManager.PlayerResourceRiseRate += RaisRate;
    }
    private void OnTriggerEnter2D(Collider2D col)
    {

        if(col.attachedRigidbody == null) {  return; }

        if(col.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            if(ownerClientId == netObj.OwnerClientId)
            {
                return;
            }
            else
            {
                ChangeOwner(netObj.OwnerClientId);
            }
            Debug.Log(ownerClientId);
        }
        
        
    }
}
