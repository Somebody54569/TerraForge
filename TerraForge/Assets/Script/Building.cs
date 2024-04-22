using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Building : NetworkBehaviour 
{
    public bool Placed;
    public BoundsInt area;
    public GridBuildingSystem _GridBuildingSystem;
    public BuildingType BuildingTypeNow;
    public Transform SpawnPoint;
    public BoundsInt areaBorder;
    //[SerializeField] private SpriteRenderer minimapIconRenderer;
    //[SerializeField] private Color ownerColorOnMap;
    
    /*public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            minimapIconRenderer.color = ownerColorOnMap;
        }
    }*/
    
    private void Start()
    {
        _GridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
        Placed = false;
    }

    public bool CanBePlaced()
    {
        Vector3Int positionInt = _GridBuildingSystem.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        if (_GridBuildingSystem.CanTakeArea(areaTemp))
        {
            return true;
        }

        return false;
    }
    
    
    
    public enum BuildingType
    {
        MotherBase,
        UnitBase,
        VehicleBase,
        Destroy
    }
}
    