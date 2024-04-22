using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    
    public GridBuildingSystem _gridBuildingSystem;
    public Building BuildingPlayerTemp;
    [SerializeField] private List<GameObject> UiPlayer;
    [SerializeField] private TMP_Text ResourceText;
    public List<UnitBehevior> SelectUnit;
    public List<GameObject> BuildingPlayer;
    public string tempBuilding;
    public int PlayerResource;
    public int PlayerResourceRiseRate;

    public bool isSpawn;
    public NetworkVariable<bool> IsPlayerMax = new NetworkVariable<bool>();
    public NetworkVariable<int> PlayerColorIndex = new NetworkVariable<int>();
    
    public List<GameObject> buildbutton;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData =
                HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerColorIndex.Value = userData.userColorIndex;
        }
    }

    private void Start()
    {
        isSpawn = false;
        foreach (var VARIABLE in buildbutton)
        {
            VARIABLE.SetActive(false);
        }
        _gridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
        if (IsOwner)
        {
            foreach (var VARIABLE in UiPlayer)
            {
                VARIABLE.SetActive(true);
            }
 
        }
        else if (IsServer)
        {
            foreach (var VARIABLE in UiPlayer)
            {
                VARIABLE.SetActive(false);
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

    private void FixedUpdate()
    {
        if (IsPlayerMax.Value == true)
        {
            float resourceIncrease = PlayerResourceRiseRate * Time.deltaTime; 
            PlayerResource += (int)resourceIncrease;  
        }
          
    }

    private void Update()
    {
        if (BuildingPlayerTemp != null)
        {
          //  Debug.Log(BuildingPlayerTemp);   
        }
        if (!IsOwner) { return; }

       // CheckBuildingIsDestroy();
        CheckBuildingIsDestroy();
        PlayerBuildingTree();
        ResourceText.text = PlayerResource.ToString();
        
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
            PlayerResource += BuildingPlayerTemp.GetComponent<AttributeUnit>().Cost;
            Destroy(BuildingPlayerTemp.gameObject);
            BuildingPlayerTemp = null;
        }
    }

    private void PlayerBuildingTree()
    {
        RemoveMissingBuildings();
        foreach (var VARIABLE in buildbutton)
        {
            VARIABLE.SetActive(false);
        }
        foreach (GameObject building in BuildingPlayer)
        { 
            switch (building.GetComponent<Building>().BuildingTypeNow)
            {
                case Building.BuildingType.MotherBase:
                    foreach (GameObject button in buildbutton)
                    {
                        if (button && button.name == "UnitBase")
                        {
                            button.SetActive(true);
                        }
                        if (button && button.name == "MeleeUnit")
                        {
                            button.SetActive(true); 
                        }
                    }
                    break;
                case Building.BuildingType.UnitBase:
                    foreach (GameObject button in buildbutton)
                    {
                        if (button && button.name == "RangeUnit")
                        {
                            button.SetActive(true); 
                        }
                        if (button && button.name == "VehicleBase")
                        {
                            button.SetActive(true); 
                        }
                    }
                    break;
                case Building.BuildingType.VehicleBase:
                    foreach (GameObject button in buildbutton)
                    {
                        if (button && button.name == "MechUnit")
                        {
                            button.SetActive(true); 
                        }
                    }
                    break;
                default:

                    break;
                
            }
        }
        
    }
    public void RemoveMissingBuildings()
    {
        List<GameObject> buildingsToRemove = new List<GameObject>();

        foreach (GameObject building in BuildingPlayer)
        {
            // Check if the building is missing (null)
            if (building == null)
            {
                buildingsToRemove.Add(building);
            }
        }

        // Remove missing buildings
        foreach (GameObject buildingToRemove in buildingsToRemove)
        {
            BuildingPlayer.Remove(buildingToRemove);
        }
    }
    public void PlaceBuilding()
    {
        Vector3Int positionInt = _gridBuildingSystem.gridLayout.LocalToCell(BuildingPlayerTemp.transform.position);
        BoundsInt areaTemp = BuildingPlayerTemp.area;
        areaTemp.position = positionInt;
        
        
        Vector3Int positionBInt = _gridBuildingSystem.gridLayout.LocalToCell(BuildingPlayerTemp.transform.position);
        BoundsInt areaBTemp = BuildingPlayerTemp.areaBorder;

// Calculate the offset to center the bounding box
        Vector3Int centerOffset = new Vector3Int(
            Mathf.FloorToInt((areaBTemp.size.x - 2) / 2),
            Mathf.FloorToInt((areaBTemp.size.y - 1) / 2),
            Mathf.FloorToInt((areaBTemp.size.z - 1) / 2)
        );

// Set the position of the bounding box centered around BuildingPlayerTemp
        areaBTemp.position = positionBInt - centerOffset;

// Pass the adjusted area to the method
        _gridBuildingSystem.TakeBArea(areaBTemp);
        
        Destroy(BuildingPlayerTemp.gameObject);
        
        _gridBuildingSystem.TakeBArea(areaBTemp);
        InitializeWithBuilding(tempBuilding, BuildingPlayerTemp.transform.position);
        TakeAreaServerRpc(areaTemp);
        
        
        BuildingPlayerTemp.Placed = true;
        BuildingPlayerTemp = null;
        //TakeAreaServerRpc(areaTemp);
    }
    public void InitializeWithBuilding(string prefabName)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            if (PlayerResource >= buildingPrefab.GetComponent<AttributeUnit>().Cost)
            {
                GameObject instantiatedObject = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
                BuildingPlayerTemp = instantiatedObject.GetComponent<Building>();
                PlayerResource -= buildingPrefab.GetComponent<AttributeUnit>().Cost;
                if (BuildingPlayerTemp != null)
                {
                    tempBuilding = prefabName;

                    _gridBuildingSystem.FollowBuilding(BuildingPlayerTemp);
                }
            }
        }
    }
    
    [ServerRpc]
    public void InitializeWithUnitServerRpc(string prefabName)
    {
        bool hasBase = false;
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);
        
        if (buildingPrefab != null)
        {
            if (PlayerResource >= buildingPrefab.GetComponent<AttributeUnit>().Cost )
            {
                Vector3 Spawnpoint = new Vector3();
                foreach (GameObject buildingT in BuildingPlayer)
                {
                    Building building = buildingT.GetComponent<Building>();
                    switch (prefabName)
                    {
                        case "Unit_Melee":
                            if (building.BuildingTypeNow == Building.BuildingType.MotherBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                hasBase = true;
                            } 
                            break;
                        case "Unit_Range":
                            if (building.BuildingTypeNow == Building.BuildingType.UnitBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                hasBase = true;
                            } 
                            break;
                        case "Unit_Vehicle":
                            if (building.BuildingTypeNow == Building.BuildingType.VehicleBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                hasBase = true;
                            } 
                            break;
                        default:
                            hasBase = false;
                            return;
                            break;
                    }
                    
                }
                if (hasBase)
                {
                    GameObject instantiatedObject = Instantiate(buildingPrefab,Spawnpoint, Quaternion.identity);
                    instantiatedObject.GetComponent<UnitBehevior>().targetPosition = Spawnpoint;
                    PlayerColor playerColorComponent = GetComponent<PlayerColor>();
                    if (playerColorComponent != null)
                    {
                        instantiatedObject.GetComponent<UnitBehevior>().unitColor = playerColorComponent.playerColor[playerColorComponent.colorIndex];
                    }
                    NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
                    PlayerResource -= buildingPrefab.GetComponent<AttributeUnit>().Cost;
                    if (networkObject != null)
                    {
                        // Assign ownership to the client that requested the initialization
                        networkObject.SpawnWithOwnership(OwnerClientId);
                    }       
                }
                InitializeWithUnitClientRpc(prefabName);
            }
        }
    }
    [ClientRpc]
     private void InitializeWithUnitClientRpc(string prefabName)
    {
        if (IsHost)
        {
            return;
        }
        bool hasBase = false;
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);
        
        if (buildingPrefab != null)
        {
            if (PlayerResource >= buildingPrefab.GetComponent<AttributeUnit>().Cost )
            {
                Vector3 Spawnpoint = new Vector3();
                foreach (GameObject buildingT in BuildingPlayer)
                {
                    Building building = buildingT.GetComponent<Building>();
                    switch (prefabName)
                    {
                        case "Unit_Melee":
                            if (building.BuildingTypeNow == Building.BuildingType.MotherBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                hasBase = true;
                            } 
                            break;
                        case "Unit_Range":
                            if (building.BuildingTypeNow == Building.BuildingType.UnitBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                hasBase = true;
                            } 
                            break;
                        case "Unit_Vehicle":
                            if (building.BuildingTypeNow == Building.BuildingType.VehicleBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                hasBase = true;
                            } 
                            break;
                        default:
                            hasBase = false;
                            return;
                            break;
                    }
                    
                }
                if (hasBase)
                {
                    PlayerResource -= buildingPrefab.GetComponent<AttributeUnit>().Cost;
                }
            }
        }
    }


    public void InitializeWithBuilding(string prefabName, Vector3 position)
    {
        InitializeWithBuildingServerRpc(prefabName,position);
        
    }
    [ServerRpc]
    public void InitializeWithBuildingServerRpc(string prefabName, Vector3 position)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);
        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab, position, Quaternion.identity);
          //  BuildingPlayerTemp = instantiatedObject.GetComponent<Building>();
            NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.SpawnWithOwnership(OwnerClientId);
                BuildingPlayer.Add(instantiatedObject);
            }
            InitializeWithBuildingClientRpc(instantiatedObject);  
        }
       
    }
    [ClientRpc]
    public void InitializeWithBuildingClientRpc(NetworkObjectReference objectReference)
    {
        if (IsHost)
        {
            return;
        }
        
        BuildingPlayer.Add(objectReference);
        
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
    
    public void CheckBuildingIsDestroy()
    {
        RemoveMissingBuildings();
        foreach (GameObject buildingt in BuildingPlayer)
        {
            Building building = buildingt.GetComponent<Building>();
            if (building.BuildingTypeNow == Building.BuildingType.Destroy)
            {
                Vector3 position = buildingt.transform.position;
                Vector3Int positionInt = new Vector3Int((int)position.x - 1, (int)position.y, (int)position.z);
                BoundsInt areaTemp = buildingt.GetComponent<Building>().area;
                areaTemp.position = positionInt;
                ClearAreaServerRpc(areaTemp);
                Destroy(buildingt);
            }
        }
    }
    [ServerRpc]
    private void ClearAreaServerRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area)
    {
        _gridBuildingSystem.ClearAreaWhenDestroy(area);
        ClearAreaClientRpc(area);
    }
    [ClientRpc]
    private void ClearAreaClientRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area)
    {
        //   if (IsOwner) { return; }
        _gridBuildingSystem.ClearAreaWhenDestroy(area);
    }
    
}