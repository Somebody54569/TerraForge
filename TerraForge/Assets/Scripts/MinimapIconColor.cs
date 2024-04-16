using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MinimapIconColor : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer minimapIconRenderer;
    [SerializeField] private Color ownerColorOnMap;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            minimapIconRenderer.color = ownerColorOnMap;
        }
    }
}
