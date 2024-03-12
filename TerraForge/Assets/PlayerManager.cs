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
    public Building BuildingPlayerTemp;
    [SerializeField] private List<GameObject> UiPlayer;
    public List<UnitBehevior> SelectUnit;
    public List<Building> BuildingPlayer;
    public string tempBuilding;
    private void Start()
    {
        _gridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
        if (IsOwner)
        {
            foreach (var VARIABLE in UiPlayer)
            {
                VARIABLE.SetActive(true);
            }
 
        }
        else
        {
            foreach (var VARIABLE in UiPlayer)
            {
                VARIABLE.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        
        if (!BuildingPlayerTemp)
        {
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject(0))
        {
            return;
        }

        if (!BuildingPlayerTemp.Placed)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = _gridBuildingSystem.gridLayout.LocalToCell(touchPos);
            if (_gridBuildingSystem.prevPos != cellPos)
            {
                BuildingPlayerTemp.transform.localPosition =
                    _gridBuildingSystem.gridLayout.CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0f));
                _gridBuildingSystem.prevPos = cellPos;
                _gridBuildingSystem.FollowBuilding(BuildingPlayerTemp);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (BuildingPlayerTemp.CanBePlaced())
            {
               PlaceBuilding();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            _gridBuildingSystem.ClearArea();
            Destroy(BuildingPlayerTemp.gameObject);
            BuildingPlayerTemp = new Building();
        }
    }
    
    
    public void PlaceBuilding()
    {
        Vector3Int positionInt = _gridBuildingSystem.gridLayout.LocalToCell(BuildingPlayerTemp.transform.position);
        BoundsInt areaTemp = BuildingPlayerTemp.area;
        areaTemp.position = positionInt;
        BuildingPlayerTemp.Placed = true;
        Destroy( BuildingPlayerTemp.gameObject);
        InitializeWithBuildingServerRpc(tempBuilding, BuildingPlayerTemp.transform.position);
        TakeAreaServerRpc(areaTemp);
        //TakeAreaServerRpc(areaTemp);
    }
    public void InitializeWithBuilding(string prefabName)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
            BuildingPlayerTemp = instantiatedObject.GetComponent<Building>();
            if (BuildingPlayerTemp != null)
            {
                tempBuilding = prefabName;
                
                _gridBuildingSystem.FollowBuilding(BuildingPlayerTemp);
            }
        }
    }
    
    [ServerRpc]
    public void InitializeWithUnitServerRpc(string prefabName)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            Vector3 Spawnpoint = new Vector3();
            foreach (Building building in BuildingPlayer)
            {
                if (building.BuildingTypeNow == Building.BuildingType.UnitBase)
                {
                    Spawnpoint = building.SpawnPoint.position;
                }
            }
            GameObject instantiatedObject = Instantiate(buildingPrefab,Spawnpoint, Quaternion.identity);
            instantiatedObject.GetComponent<UnitBehevior>().targetPosition = Spawnpoint;
            NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Assign ownership to the client that requested the initialization
                networkObject.SpawnWithOwnership(OwnerClientId);
            }
        }
    }
    [ServerRpc]
    public void InitializeWithBuildingServerRpc(string prefabName, Vector3 position)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab, position, Quaternion.identity);
            BuildingPlayerTemp = instantiatedObject.GetComponent<Building>();
            NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
            BuildingPlayer.Add(BuildingPlayerTemp);
            if (networkObject != null)
            {
                networkObject.SpawnWithOwnership(OwnerClientId);
            }
            BuildingPlayerTemp = null;

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