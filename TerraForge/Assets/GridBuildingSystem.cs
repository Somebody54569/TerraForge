using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Unity.Netcode;
public class GridBuildingSystem : NetworkBehaviour
{
//    public static GridBuildingSystem current;

    public GridLayout gridLayout;
    public Tilemap mainTileMap;
    public Tilemap TempTileMap;

    private static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    public Building temp;
    public Vector3 prevPos;
    private BoundsInt prevArea;

    // Start is called before the first frame update
    
    void Start()
    {
        gridLayout = GameObject.Find("Grid").GetComponent<GridLayout>();
        mainTileMap = GameObject.Find("MainTilemap").GetComponent<Tilemap>();
        TempTileMap = GameObject.Find("TempTilemap").GetComponent<Tilemap>();
        string tilePath = @"TileMap\";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "White"));
        tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "Green"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "Red"));
        /*// Check if the key already exists before adding
        if (!tileBases.ContainsKey(TileType.Empty))
        {
            tileBases.Add(TileType.Empty, null);
        }

        if (!tileBases.ContainsKey(TileType.White))
        {
            tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "White"));
        }

        if (!tileBases.ContainsKey(TileType.Green))
        {
            tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "Green"));
        }

        if (!tileBases.ContainsKey(TileType.Red))
        {
            tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "Red"));
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        

    }
    
   /* [ServerRpc]
    public void InitializeWithBuildingServerRpc(string prefabName)
    {
        InitializeWithBuilding(prefabName);
        InitializeWithBuildingClientRpc(prefabName);
    }

    [ClientRpc]
    public void InitializeWithBuildingClientRpc(string prefabName)
    {
        if (IsOwner) { return; }
        InitializeWithBuilding(prefabName);
        
    }
*/
    public void InitializeWithBuilding(string prefabName)
    {
        GameObject buildingPrefab = Resources.Load<GameObject>(prefabName);

        if (buildingPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
            temp = instantiatedObject.GetComponent<Building>();
            if (temp != null)
            {
                temp._GridBuildingSystem = this;
                FollowBuilding();
            }
        }
    }
    /*
    public void InitializeWithBuilding(GameObject building)
    {
        temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        temp._GridBuildingSystem = this;
        FollowBuilding();
    }
    */
    
 

    public void FollowBuilding()
    {
        ClearArea();
        temp.area.position = gridLayout.WorldToCell(temp.gameObject.transform.position);
        BoundsInt buildingArea = temp.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, mainTileMap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];
        for (int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == tileBases[TileType.White])
            {
                tileArray[i] = tileBases[TileType.Green];
            }
            else
            {
                FillTiles(tileArray, TileType.Red);
                break;
            }
        }
        TempTileMap.SetTilesBlock(buildingArea, tileArray);
        prevArea = buildingArea;
    }

    public bool CanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, mainTileMap);
        foreach (var b in baseArray)
        {
            if (b != tileBases[TileType.White])
            {
                return false;
            }
        }

        return true;
    }
    
 
    
    public void TakeArea(BoundsInt area)
    {
        SetTilesBlock(area, TileType.Empty, TempTileMap);
        SetTilesBlock(area, TileType.Green, mainTileMap);
    }

    public void ClearArea()
    {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear, TileType.Empty);
        TempTileMap.SetTilesBlock(prevArea, toClear);
    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    private static void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = tileBases[type];
        }
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }
}

public enum TileType
{
    Empty,
    White,
    Green,
    Red,
}
