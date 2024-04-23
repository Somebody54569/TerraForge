using System;
using System.Collections;
using System.Collections.Generic;
using BarthaSzabolcs.Tutorial_SpriteFlash;
using UnityEngine;
using Unity.Netcode;

public class Building : NetworkBehaviour 
{
    public bool Placed;
    public BoundsInt area;
    public GridBuildingSystem _GridBuildingSystem;
    public BuildingType BuildingTypeNow;
    public stateBuilding stateBuildingTypeNow;
    public Transform SpawnPoint;
    public BoundsInt areaBorder;

    public SpriteRenderer SpriteRenderer;
    public Color unitColor;
    private AttributeUnit attributeUnit;
 
    public List<GameObject> TargetToAttack;
    public GameObject CurrentTarget;

    public SimpleFlash SimpleFlash;
    public GameObject light;
    public float AttackRange;
    [SerializeField] private CircleCollider2D DetectRange;
    //[SerializeField] private SpriteRenderer minimapIconRenderer;
    //[SerializeField] private Color ownerColorOnMap;
    
    /*public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            minimapIconRenderer.color = ownerColorOnMap;
        }
    }*/


    
    public void SetTarget(GameObject newTarget)
    {
        TargetToAttack.Add(newTarget);
    }
    public void RemoveTarget(GameObject newTarget)
    {
        TargetToAttack.Remove(newTarget);
    }
    
    public void RemoveMissingBuildings()
    {
        List<GameObject> buildingsToRemove = new List<GameObject>();

        foreach (GameObject building in TargetToAttack)
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
            TargetToAttack.Remove(buildingToRemove);
        }
    }

    private void Start()
    {
        TargetToAttack = new List<GameObject>();
        _GridBuildingSystem = FindAnyObjectByType<GridBuildingSystem>();
        attributeUnit = this.GetComponent<AttributeUnit>();
        SimpleFlash.DefulColor = unitColor;
        SpriteRenderer.color = unitColor;
        stateBuildingTypeNow = stateBuilding.Work;
        Placed = false;
        AttackRange = attributeUnit.AttackRange;
        DetectRange.radius = AttackRange + 1.5f;
    }

    private void Attack()
    {
        if (attributeUnit.Dmg != 0)
        {
            if (attributeUnit.timeSinceLastAttack == 0f)
            {
                attributeUnit.flash.SetActive(true);
                attributeUnit.SetMuzzleServerRpc(false);
                AudioManager.Instance.PlaySFX("Attack");
            }

            AttackRange = attributeUnit.AttackRange;
            DetectRange.radius = AttackRange;
        
            attributeUnit.timeSinceLastAttack += Time.deltaTime;
            if (attributeUnit.timeSinceLastAttack >= attributeUnit.AttackCooldown)
            {
                // TargetToAttack.GetComponent<SimpleFlash>().Flash();
                attributeUnit.flash.SetActive(false);
                attributeUnit.SetMuzzleServerRpc(true);
                DamageToTargetServerRpc();
                attributeUnit.timeSinceLastAttack = 0f;
            }          
        }
  
    }

    private void FixedUpdate()
    {
        RemoveMissingBuildings();
        if (!IsOwner) { return; }
        if (CurrentTarget != null)
        {
            Attack();
        }
        else
        {
            attributeUnit.SetMuzzleServerRpc(false);
        }


    }


    [ServerRpc]
    private void DamageToTargetServerRpc()
    {
      //  FlashServerRpc();
        CurrentTarget.GetComponent<AttributeUnit>().TakeDamage(attributeUnit.Dmg);
    }
    

    public bool CanBePlaced()
    {
        Vector3Int positionInt = _GridBuildingSystem.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        switch (BuildingTypeNow)
        {
            case BuildingType.VoridiumDrill:
                areaTemp.position = positionInt;
                if (_GridBuildingSystem.CanTakeArea(areaTemp,TileType.Blue))
                {
                    return true;
                }
                break;
            default:
              
                areaTemp.position = positionInt;
                if (_GridBuildingSystem.CanTakeArea(areaTemp,TileType.White))
                {
                    return true;
                }
                break;
        }
        
        return false;
    }
    
    
    
    public enum BuildingType
    {
        MotherBase,
        UnitBase,
        VehicleBase,
        PillBox,
        VoridiumDrill,
    }
    public enum stateBuilding
    {
        Work,
        Destroy
    }
}
    