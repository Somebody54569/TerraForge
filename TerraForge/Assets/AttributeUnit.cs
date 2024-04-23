using System;
using System.Collections;
using System.Collections.Generic;
using BarthaSzabolcs.Tutorial_SpriteFlash;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class AttributeUnit : NetworkBehaviour
{
  
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;

    private Building _building;
    [SerializeField] public int Armor;
    [SerializeField] public int Cost;
    [SerializeField] public int Dmg;
    public float timeSinceLastAttack = 0f;
    [SerializeField] public float AttackCooldown;
    public float AttackRange;
    private bool isDead;

    [SerializeField] private GameObject Armorup;
 //   [SerializeField] public float CDtoBuild;

    private void Start()
    {
        if (this.GetComponent<Building>() != null)
        {
            _building = this.GetComponent<Building>();      
        }
    }

    private void FixedUpdate()
    {
        if (Armor > 0)
        {
            Armorup.SetActive(true);
        }
        else
        {
            Armorup.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        CurrentHealth.Value = MaxHealth;
    }

    

    public void TakeDamage(int damageValue)
    {
        int Result =  damageValue - Random.Range(1,Armor);
        ModifyHealth(-Result);
       // this.GetComponent<SimpleFlash>().Flash();
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
                AudioManager.Instance.PlaySFX("Died");
                Destroy(this.gameObject);
            }
            if (this.GetComponent<Building>() != null)
            {
                AudioManager.Instance.PlaySFX("Collapse");
                _building.stateBuildingTypeNow = Building.stateBuilding.Destroy;
            }
        }
    }


}