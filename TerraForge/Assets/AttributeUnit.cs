using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AttributeUnit : NetworkBehaviour
{
    public NetworkVariable<int> CurrentCost = new NetworkVariable<int>();
    [field: SerializeField] public int MaxCost { get; private set; } = 100;
    
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    
    public NetworkVariable<int> CurrentDmg = new NetworkVariable<int>();
    [field: SerializeField] public int MaxDmg { get; private set; } = 100;

    public int AttackRange;
    private bool isDead;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        CurrentHealth.Value = MaxHealth;
        CurrentCost.Value = MaxCost;
        CurrentDmg.Value = MaxDmg;
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
            Destroy(this.gameObject);
        }
    }


}