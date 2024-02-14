using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Building : NetworkBehaviour 
{
    public bool Placed;
    public BoundsInt area;
    public GridBuildingSystem _GridBuildingSystem;

    private ulong ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
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
    
}
    