using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerManager : NetworkBehaviour
{
    
    public GridBuildingSystem _gridBuildingSystem;

    [SerializeField] private GameObject UiPlayer;

    public string tempBuilding;
    private void Start()
    {
        _gridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
        if (IsOwner)
        {
            UiPlayer.SetActive(true);
        }
        else
        {
            UiPlayer.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        
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
        InitializeWithBuildingServerRpc(tempBuilding, _gridBuildingSystem.temp.transform.position);
        Destroy( _gridBuildingSystem.temp.gameObject);
  
        
        //TakeAreaServerRpc(areaTemp);
    }
    public void InitializeWithBuilding(string prefabName)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
            _gridBuildingSystem.temp = instantiatedObject.GetComponent<Building>();
            if (_gridBuildingSystem.temp != null)
            {
                _gridBuildingSystem.temp._GridBuildingSystem = this;
                tempBuilding = prefabName;
                _gridBuildingSystem.FollowBuilding();
            }
        }
    }
    
    [ServerRpc]
    public void InitializeWithUnitServerRpc(string prefabName)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
            NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Assign ownership to the client that requested the initialization
                networkObject.SpawnWithOwnership(OwnerClientId);
            }
        }
    }
    [ServerRpc]
    public void InitializeWithBuildingServerRpc(string prefabName ,Vector3 positiob)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab,positiob, Quaternion.identity);
            _gridBuildingSystem.temp = instantiatedObject.GetComponent<Building>();
            NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Assign ownership to the client that requested the initialization
                networkObject.SpawnWithOwnership(OwnerClientId);
                if (_gridBuildingSystem.temp != null)
                {
                    _gridBuildingSystem.temp._GridBuildingSystem = this;
                }
            }
            
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