using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AttributeUnit : NetworkBehaviour
{
  
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;

    private Building _building;
    [SerializeField] public int Cost;
    [SerializeField] public int Dmg;
    public float timeSinceLastAttack = 0f;
    [SerializeField] public float AttackCooldown;
    public float AttackRange;
    private bool isDead;
    
 //   [SerializeField] public float CDtoBuild;

    private void Start()
    {
        if (this.GetComponent<Building>() != null)
        {
            _building = this.GetComponent<Building>();      
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        CurrentHealth.Value = MaxHealth;
    }

    

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if (isDead) { return; }

        int newHealth = CurrentHealth.Value + value;
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);
        if (CurrentHealth.Value == 0)
        {
            if (this.GetComponent<Building>() == null)
            {
                Destroy(this.gameObject);
            }
            if (this.GetComponent<Building>() != null)
            {
                _building.BuildingTypeNow = Building.BuildingType.Destroy;
            }
        }
    }


}