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
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<AttributeUnit>()!= null)
        {
            building.RemoveTarget(other.gameObject);
        }
    }

    private void FixedUpdate()
    {
        float nearestDistance = Mathf.Infinity; // Initialize the nearest distance to a very large value
        GameObject nearestTarget = null; // Initialize the nearest target to null

        foreach (var target in building.TargetToAttack)
        {
            float distanceToTarget = Vector2.Distance(building.transform.position, target.transform.position);

            if (distanceToTarget < building.AttackRange && distanceToTarget < nearestDistance)
            {
                nearestDistance = distanceToTarget;
                nearestTarget = target;
            }
        }

        // Set the current target to the nearest one found
        building.CurrentTarget = nearestTarget;
    }

}
