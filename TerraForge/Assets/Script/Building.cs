using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Building : NetworkBehaviour 
{
    public bool Placed;
    public BoundsInt area;
    public PlayerManager _GridBuildingSystem;
    
    public bool CanBePlaced()
    {
        Vector3Int positionInt = _GridBuildingSystem._gridBuildingSystem.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        if (_GridBuildingSystem._gridBuildingSystem.CanTakeArea(areaTemp))
        {
            return true;
        }

        return false;
    }
    
}
    