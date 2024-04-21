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
    public List<Building> BuildingPlayer;
    public string tempBuilding;
    public int PlayerResource;
    public int PlayerResourceRiseRate;
    
    //Color
    [SerializeField] private SpriteRenderer playerSprites;
    [SerializeField] private Color[] playerColor;
    [SerializeField] private int colorIndex;
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
        HandlePlayerColorChanged(0, PlayerColorIndex.Value);
        PlayerColorIndex.OnValueChanged += HandlePlayerColorChanged;
    }

    private void FixedUpdate()
    {
        float resourceIncrease = PlayerResourceRiseRate * Time.deltaTime;
        PlayerResource += (int)resourceIncrease;    
    }

    private void Update()
    {

        if (!IsOwner) { return; }
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
        foreach (Building building in BuildingPlayer)
        { 
            switch (building.BuildingTypeNow)
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
        List<Building> buildingsToRemove = new List<Building>();

        foreach (Building building in BuildingPlayer)
        {
            // Check if the building is missing (null)
            if (building == null)
            {
                buildingsToRemove.Add(building);
            }
        }

        // Remove missing buildings
        foreach (Building buildingToRemove in buildingsToRemove)
        {
            BuildingPlayer.Remove(buildingToRemove);
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
    
    private void HandlePlayerColorChanged(int oldIndex, int newIndex)
    {
        colorIndex = newIndex;
    }
    
    private void OnDestroy()
    {
        PlayerColorIndex.OnValueChanged -= HandlePlayerColorChanged;
    }
    
    [ServerRpc]
    public void InitializeWithUnitServerRpc(string prefabName)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);
        
        if (buildingPrefab != null)
        {
            if (PlayerResource >= buildingPrefab.GetComponent<AttributeUnit>().Cost )
            {
                Vector3 Spawnpoint = new Vector3();
                foreach (Building building in BuildingPlayer)
                {
                    switch (prefabName)
                    {
                        case "Unit_Melee":
                            if (building.BuildingTypeNow == Building.BuildingType.MotherBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                GameObject instantiatedObject = Instantiate(buildingPrefab,Spawnpoint, Quaternion.identity);
                                instantiatedObject.GetComponent<UnitBehevior>().targetPosition = Spawnpoint;
                                NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
                                PlayerResource -= buildingPrefab.GetComponent<AttributeUnit>().Cost;
                                if (networkObject != null)
                                {
                                    // Assign ownership to the client that requested the initialization
                                    networkObject.SpawnWithOwnership(OwnerClientId);
                                }

                                playerSprites = instantiatedObject.GetComponent<SpriteRenderer>();
                                playerSprites.color = playerColor[colorIndex];
                            } 
                            break;
                        case "Unit_Range":
                            if (building.BuildingTypeNow == Building.BuildingType.UnitBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                GameObject instantiatedObject = Instantiate(buildingPrefab,Spawnpoint, Quaternion.identity);
                                instantiatedObject.GetComponent<UnitBehevior>().targetPosition = Spawnpoint;
                                NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
                                PlayerResource -= buildingPrefab.GetComponent<AttributeUnit>().Cost;
                                if (networkObject != null)
                                {
                                    // Assign ownership to the client that requested the initialization
                                    networkObject.SpawnWithOwnership(OwnerClientId);
                                }

                                playerSprites = instantiatedObject.GetComponent<SpriteRenderer>();
                                playerSprites.color = playerColor[colorIndex];
                            } 
                            break;
                        case "Unit_Vehicle":
                            if (building.BuildingTypeNow == Building.BuildingType.VehicleBase)
                            {
                                Spawnpoint = building.SpawnPoint.position;
                                GameObject instantiatedObject = Instantiate(buildingPrefab,Spawnpoint, Quaternion.identity);
                                instantiatedObject.GetComponent<UnitBehevior>().targetPosition = Spawnpoint;
                                NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
                                PlayerResource -= buildingPrefab.GetComponent<AttributeUnit>().Cost;
                                if (networkObject != null)
                                {
                                    // Assign ownership to the client that requested the initialization
                                    networkObject.SpawnWithOwnership(OwnerClientId);
                                }

                                playerSprites = instantiatedObject.GetComponent<SpriteRenderer>();
                                playerSprites.color = playerColor[colorIndex];
                            } 
                            break;
                        default:
                            return;
                            break;
                    }
                   
                }

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
            if (networkObject != null)
            {
                networkObject.SpawnWithOwnership(OwnerClientId);
            }

            if (networkObject.OwnerClientId == this.OwnerClientId)
            {
                BuildingPlayer.Add(BuildingPlayerTemp);
            } 
            BuildingPlayerTemp = null;

        }
        InitializeWithBuildingClientRpc(prefabName);
    }
    [ClientRpc]
    public void InitializeWithBuildingClientRpc(string prefabName)
    {
        if (IsServer)
        {
            return;
        }
        
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            Debug.Log("ADD");
            Debug.Log(BuildingPlayerTemp);
            BuildingPlayer.Add(BuildingPlayerTemp);
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
    
    
    [ServerRpc]
    private void ClearAreaServerRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area)
    {
        _gridBuildingSystem.ClearAreaWhenDestroy(area);
        TakeAreaClientRpc(area);
    }
    [ClientRpc]
    private void ClearAreaClientRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area)
    {
        //   if (IsOwner) { return; }
        _gridBuildingSystem.ClearAreaWhenDestroy(area);
    }
    
}