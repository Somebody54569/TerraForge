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
    
    public List<GameObject> UnitPlayer;
    public string tempBuilding;
    public int PlayerResource;
    public int PlayerResourceRiseRate;

    public bool IsLose;
    public NetworkVariable<bool> IsPlayerMax = new NetworkVariable<bool>();
    public NetworkVariable<int> PlayerColorIndex = new NetworkVariable<int>();
    
    public List<GameObject> buildbutton;

  //  private List<Building> Vbuilding;

  private int unitsPerCircle = 4; // Define the initial number of units per circle

  private void SetTargetPositionsAroundMouse()
  {
      int newUnitsPerCircle = unitsPerCircle;
      Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      int count = SelectUnit.Count;

      int unitsDistributed = 0;
      int circleCount = 1; // Start with one circle

      while (unitsDistributed < count)
      {
          // Calculate the number of units for this circle
          int unitsInThisCircle = Mathf.Min(newUnitsPerCircle, count - unitsDistributed);

          // Calculate the radius step between circles
          float radiusStep = circleCount * 0.7f; // Adjust this value for the spacing between circles

          // Loop through each unit in this circle
          for (int i = 0; i < unitsInThisCircle; i++)
          {
              // Calculate position based on angle and radius
              float angle = i * (360f / newUnitsPerCircle);
              float posX = mousePosition.x + radiusStep * Mathf.Cos(angle * Mathf.Deg2Rad);
              float posY = mousePosition.y + radiusStep * Mathf.Sin(angle * Mathf.Deg2Rad);
              Vector3 targetPos = new Vector3(posX, posY, mousePosition.z);

              // Set target position for the unit
              SelectUnit[unitsDistributed].targetPosition = targetPos;
              SelectUnit[unitsDistributed].isSetToForceMove = true;
              SelectUnit[unitsDistributed].currentUnitState = UnitState.Walk;

              unitsDistributed++;
          }
        
          // Update the number of units per circle for the next circle
          newUnitsPerCircle *= 2;

          // Calculate the number of circles needed for the remaining units
          circleCount++;
      }
  }





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
     //   Vbuilding = new List<Building>();
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
            float resourceIncrease = PlayerResourceRiseRate  *Time.deltaTime; 
            PlayerResource += (int)resourceIncrease;  
           // Debug.Log(Vbuilding.Count);
        }
        
    }

    private void Update()
    {
        RemoveMissingUnit();
        RemoveMissingBuildings();
        if (!IsOwner) { return; }
        CheckBuildingIsDestroy();
        
        if (Input.GetMouseButtonDown(1))
        {
            foreach (UnitBehevior unit in SelectUnit)
            {
                if (unit.currentState == state.Select)
                {
                    if (SelectUnit.Count > 1)
                    {
                        SetTargetPositionsAroundMouse();
                        return;
                    }

                    // Otherwise, set targetPosition to mousePosition
                    unit.targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    unit. isSetToForceMove = true;
                    unit.currentUnitState = UnitState.Walk;
                }
            }
        }
       // CheckBuildingIsDestroy();
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
                switch (BuildingPlayerTemp.BuildingTypeNow)
                {
                    case Building.BuildingType.VoridiumDrill:
                        _gridBuildingSystem.FollowBuilding(BuildingPlayerTemp,TileType.Blue);
                        break;
                    default:
                        _gridBuildingSystem.FollowBuilding(BuildingPlayerTemp,TileType.White);
                        break;
                }
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

    public void PlayerSpawnWithBase(Vector3 position)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>("MotherBaseBuilding");
        Building StartBuild = buildingPrefab.GetComponent<Building>();
        tempBuilding = "MotherBaseBuilding";
        
        Vector3Int positionInt = _gridBuildingSystem.gridLayout.LocalToCell(position);
        BoundsInt areaTemp = StartBuild.area;
        areaTemp.position = positionInt;
        
    

// Pass the adjusted area to the method
    
        InitializeWithBuilding(tempBuilding, position);
        
        Vector3Int positionBInt = _gridBuildingSystem.gridLayout.LocalToCell(position);
        BoundsInt areaBTemp = StartBuild.areaBorder;

// Calculate the offset to center the bounding box
        Vector3Int centerOffset = new Vector3Int(
            Mathf.FloorToInt((areaBTemp.size.x - 2) / 2),
            Mathf.FloorToInt((areaBTemp.size.y - 1) / 2),
            Mathf.FloorToInt((areaBTemp.size.z - 1) / 2)
        );
// Set the position of the bounding box centered around BuildingPlayerTemp
        areaBTemp.position = positionBInt - centerOffset;
        StartBuild.Placed = true;

        if (!IsOwner)
        {
            return;
        }
        _gridBuildingSystem.TakeBArea(areaBTemp);
        
        TakeAreaServerRpc(areaTemp);
        
        
   
        //TakeAreaServerRpc(areaTemp);
    }

    private void calCoin()
    {
        PlayerResourceRiseRate = 100;
        foreach (GameObject building in BuildingPlayer)
        {
            if (building.GetComponent<Building>().BuildingTypeNow == Building.BuildingType.VoridiumDrill)
            {
                PlayerResourceRiseRate += 50;
            }
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
                        if (button && button.name == "VoridiumDrill")
                        {
                            button.SetActive(true); 
                        }
                    }
                    break;
                case Building.BuildingType.UnitBase:
                    foreach (GameObject button in buildbutton)
                    {
                        if (button && button.name == "PillBox")
                        {
                            button.SetActive(true); 
                        }
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
                case Building.BuildingType.VoridiumDrill:
                    foreach (GameObject button in buildbutton)
                    {
                        if (button && button.name == "VoridiumDrill")
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
    public void RemoveMissingUnit()
    {
        List<GameObject> buildingsToRemove = new List<GameObject>();

        foreach (GameObject building in UnitPlayer)
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
            UnitPlayer.Remove(buildingToRemove);
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
        calCoin();
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
                    BuildingPlayerTemp.GetComponent<BoxCollider2D>().enabled = false;
                    BuildingPlayerTemp.GetComponent<Building>().light.SetActive(false);
                    switch (BuildingPlayerTemp.BuildingTypeNow)
                    {
                        case Building.BuildingType.VoridiumDrill:
                            _gridBuildingSystem.FollowBuilding(BuildingPlayerTemp,TileType.Blue);
                            break;
                        default:
                            _gridBuildingSystem.FollowBuilding(BuildingPlayerTemp,TileType.White);
                            break;
                    }
                    
                }
            }
        }
    }
    
    public void InitializeWithUnit(string prefabName)
    {
        bool hasBase = false;
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);
        if (PlayerResource >= buildingPrefab.GetComponent<AttributeUnit>().Cost)
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
                InitializeWithUnitServerRpc(prefabName);
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
                GameObject instantiatedObject = Instantiate(buildingPrefab, Spawnpoint, Quaternion.identity);
                instantiatedObject.GetComponent<UnitBehevior>().targetPosition = Spawnpoint;
                PlayerColor playerColorComponent = GetComponent<PlayerColor>();
                if (playerColorComponent != null)
                {
                    instantiatedObject.GetComponent<UnitBehevior>().unitColor =
                        playerColorComponent.playerColor[playerColorComponent.colorIndex];
                }

                NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
              //  PlayerResource -= buildingPrefab.GetComponent<AttributeUnit>().Cost;
                if (networkObject != null)
                {
                    // Assign ownership to the client that requested the initialization
                    networkObject.SpawnWithOwnership(OwnerClientId);
                }

                InitializeWithUnitClientRpc(instantiatedObject, prefabName);
            }

        }
    }
    [ClientRpc]
     private void InitializeWithUnitClientRpc(NetworkObjectReference unit , string prefabName)
    {
        if (IsHost)
        {
            return;
        }
        UnitPlayer.Add(unit);
        
        foreach (var VARIABLE in UnitPlayer)
        {
            PlayerColor playerColorComponent = GetComponent<PlayerColor>();
            if (playerColorComponent != null)
            {
                VARIABLE.GetComponent<UnitBehevior>().unitColor = playerColorComponent.playerColor[playerColorComponent.colorIndex];
            }
        }
  
    }


    public void InitializeWithBuilding(string prefabName, Vector3 position)
    {
        InitializeWithBuildingServerRpc(prefabName,position);
        AudioManager.Instance.PlaySFX("Button");
        
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
            PlayerColor playerColorComponent = GetComponent<PlayerColor>();
            if (playerColorComponent != null)
            {
                instantiatedObject.GetComponent<Building>().unitColor = playerColorComponent.playerColor[playerColorComponent.colorIndex];
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
        foreach (var VARIABLE in BuildingPlayer)
        {
            PlayerColor playerColorComponent = GetComponent<PlayerColor>();
            if (playerColorComponent != null)
            {
                VARIABLE.GetComponent<Building>().unitColor = playerColorComponent.playerColor[playerColorComponent.colorIndex];
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
    
    public void CheckBuildingIsDestroy()
    {
        RemoveMissingBuildings();
        foreach (GameObject buildingt in BuildingPlayer)
        {
            Building building = buildingt.GetComponent<Building>();
            if (building.BuildingTypeNow == Building.BuildingType.VoridiumDrill)
            {
                if (building.stateBuildingTypeNow == Building.stateBuilding.Destroy)
                {
                    Vector3 position = buildingt.transform.position;
                    Vector3Int positionInt = new Vector3Int((int)position.x, (int)position.y -1, (int)position.z);
                    BoundsInt areaTemp = buildingt.GetComponent<Building>().area;
                    areaTemp.position = positionInt;
                    ClearAreaServerRpc(areaTemp,TileType.Blue);
                    _gridBuildingSystem.ClearAreaWhenDestroy(areaTemp , TileType.Blue);
                    explosionEff("explosion",position);
                    //Destroy(buildingt);
                    DestoryObjServer(buildingt);
                    //building.DestroyBuilding(buildingt);
                }
            }
            else
            {
                if (building.stateBuildingTypeNow == Building.stateBuilding.Destroy)
                {
                    Vector3 position = buildingt.transform.position;
                    Vector3Int positionInt = new Vector3Int((int)position.x - 1, (int)position.y, (int)position.z);
                    BoundsInt areaTemp = buildingt.GetComponent<Building>().area;
                    areaTemp.position = positionInt;
                    if (building.BuildingTypeNow == Building.BuildingType.MotherBase)
                    {
                        ImLose(true);
                    }
                    ClearAreaServerRpc(areaTemp,TileType.White);
                    explosionEff("explosion",position);
                    _gridBuildingSystem.ClearAreaWhenDestroy(areaTemp , TileType.White);
                    DestoryObjServer(buildingt);
                    //building.DestroyBuilding(buildingt);
                }
            }
            
        }

        calCoin();
    }
/*
    [ServerRpc]
    private void CheckBuildingIsDestroyServerRpc()
    {
        if (!IsOwner)
        {
            return;
        }
        CheckBuildingIsDestroyClientRpc();
    }

     [ClientRpc]
    private void CheckBuildingIsDestroyClientRpc()
    {
        
    }
    */
    private void DestoryObjServer(GameObject obj)
    {
        DestoryObjServerRpc(obj);
    }

    [ServerRpc]
    private void DestoryObjServerRpc(NetworkObjectReference objF)
    {
        GameObject gameObjectF = objF;
        Destroy(gameObjectF);
        DestoryObjClientRpc(objF);
    }
    [ClientRpc]
    private void DestoryObjClientRpc(NetworkObjectReference objF)
    {
        GameObject gameObjectF = objF;
        Destroy(gameObjectF);
    }


    private void explosionEff(string prefabName,Vector3 position)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);
        if (buildingPrefab != null)
        {
            Instantiate(buildingPrefab, position,Quaternion.identity);
            explosionServerRpc(prefabName,position);
        }
    }
    [ServerRpc]
    private void explosionServerRpc(string prefabName,Vector3 position)
    {
        if (IsOwner)
        {
            GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);
            Instantiate(buildingPrefab, position,Quaternion.identity);
        }
        explosionClientRpc(prefabName,position);
    }
    [ClientRpc]
    private void explosionClientRpc(string prefabName,Vector3 position)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);
        Instantiate(buildingPrefab, position,Quaternion.identity);
    }
    
    [ServerRpc]
    private void ClearAreaServerRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area,TileType type)
    {
        _gridBuildingSystem.ClearAreaWhenDestroy(area , type);
        ClearAreaClientRpc(area,type);
    }
    [ClientRpc]
    private void ClearAreaClientRpc(ForceNetworkSerializeByMemcpy<BoundsInt> area ,TileType type)
    {

        _gridBuildingSystem.ClearAreaWhenDestroy(area ,type);
    }
    private void ImLose(bool b)
    {
        IsLose = b;
        ImLoseServerRpc(b);
        
    }

    [ServerRpc]
    private void ImLoseServerRpc(bool b)
    {
        ImLoseClientRpc(b);
    }

    [ClientRpc]
    private void ImLoseClientRpc(bool b)
    {
        IsLose = b;
    }
}