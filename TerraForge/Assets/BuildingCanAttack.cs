using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildingCanAttack : MonoBehaviour
{
    private ulong ownerClientId;
    [SerializeField] private Building building;

    private void Start()
    {
        this.ownerClientId = building.OwnerClientId;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.attachedRigidbody == null) {  return; }

        if(col.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            if(ownerClientId == netObj.OwnerClientId)
            {
                return;
            }
        }

        if (col.GetComponent<AttributeUnit>()!= null)
        {
            building.SetTarget(col.gameObject);

        }
    }

    private void FixedUpdate()
    {
        foreach (var target in building.TargetToAttack)
        {
            if (Vector2.Distance( transform.position, target.transform.position) < building.AttackRange)
            {
                building.CurrentTarget = target;

            }
        }

    }
}
