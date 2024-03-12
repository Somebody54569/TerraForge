using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class DetectAndDmg : MonoBehaviour
{
    private ulong ownerClientId;
    [SerializeField] private UnitBehevior Unit;

    private void Start()
    {
        this.ownerClientId = Unit.OwnerClientId;
    }

    private void OnTriggerStay2D(Collider2D col)
    {

        if(col.attachedRigidbody == null) {  return; }

        if(col.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            if(ownerClientId == netObj.OwnerClientId)
            {
                return;
            }
        }

        if (col.GetComponent<UnitBehevior>()!= null)
        {
            if (Unit.TargetToAttack == null)
            {
                Unit.SetTarget(col.gameObject);      
            }
          
        }
     
    }
}
