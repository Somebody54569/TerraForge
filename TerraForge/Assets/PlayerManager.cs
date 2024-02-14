using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class PlayerManager : NetworkBehaviour
{
    
    private GridBuildingSystem _gridBuildingSystem;
    

    private void Start()
    {
        _gridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            InitializeWithUnitServerRpc("Unit_Melee");
        }
        if (!_gridBuildingSystem.temp)
        {
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject(0))
        {
            return;
        }

        if (!_gridBuildingSystem.temp.Placed)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = _gridBuildingSystem.gridLayout.LocalToCell(touchPos);
            if (_gridBuildingSystem.prevPos != cellPos)
            {
                _gridBuildingSystem.temp.transform.localPosition =
                    _gridBuildingSystem.gridLayout.CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0f));
                _gridBuildingSystem.prevPos = cellPos;
                _gridBuildingSystem.FollowBuilding();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (_gridBuildingSystem.temp.CanBePlaced())
            {
               PlaceBuilding();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            _gridBuildingSystem.ClearArea();
            Destroy(_gridBuildingSystem.temp.gameObject);
        }
    }
    public void PlaceBuilding()
    {
        Vector3Int positionInt = _gridBuildingSystem.gridLayout.LocalToCell(_gridBuildingSystem.temp.transform.position);
        BoundsInt areaTemp = _gridBuildingSystem.temp.area;
        areaTemp.position = positionInt;
        _gridBuildingSystem.temp.Placed = true;
        TakeAreaServerRpc(areaTemp);
        //TakeAreaServerRpc(areaTemp);
    }

    [ServerRpc]
    public void InitializeWithUnitServerRpc(string prefabName)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
            instantiatedObject.GetComponent<NetworkObject>().Spawn();
            // Check if the instantiated object has a NetworkObject component
            
        }
        InitializeWithUnitClientRpc(prefabName);
    }
    [ClientRpc]
    public void InitializeWithUnitClientRpc(string prefabName)
    {
       // if (IsOwner) { return; }
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
            instantiatedObject.GetComponent<NetworkObject>().Spawn();
        }
    }
    [ServerRpc]
    private void TakeAreaServerRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area)
    {
        _gridBuildingSystem.TakeArea(area);
        TakeAreaClientRpc(area);
    }
    [ClientRpc]
    private void TakeAreaClientRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area)
    {
     //   if (IsOwner) { return; }
        _gridBuildingSystem.TakeArea(area);
    }
}