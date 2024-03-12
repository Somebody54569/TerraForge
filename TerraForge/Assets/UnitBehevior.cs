using System;
using System.Collections;
using System.Collections.Generic;
using BarthaSzabolcs.Tutorial_SpriteFlash;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Attribute = System.Attribute;

public class UnitBehevior : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float movementSpeed = 4f;
    public Vector2 targetPosition;
    public SpriteRenderer SpriteRenderer;
    public Animator Animator;
    public NetworkAnimator NetworkAnimator;
    private state currentState;
    [SerializeField] private GameObject SelectIcon;
    private float AttackRange;
    public AttributeUnit attributeUnit;
    public GameObject TargetToAttack;
    [SerializeField] private CircleCollider2D DetectRange;
    private bool isSetToForceMove;
    private void Start()
    {
       ChangeState(state.UnSelect);
       attributeUnit = this.GetComponent<AttributeUnit>();
       AttackRange = attributeUnit.AttackRange;
       DetectRange.radius = AttackRange + 1.5f;

    }
    

    private void Update()
    {
        if (!IsOwner)
        {
            SelectIcon.SetActive(false);
            return;
        }
        
            if (Input.GetMouseButtonDown(1))
            {
                if (currentState == state.Select)
                {
                    SetTarget(null);
                    targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    isSetToForceMove = true;
                }
            }
            CheckIconSelect();
    }

    private void FixedUpdate()
    {
       if (!IsOwner) { return; }

       if (TargetToAttack != null)
       {
           if (!isSetToForceMove)
           {
               MoveToTargetAndAttack();    
           }
           else
           {
               MoveTo();
           }
                
       }
       else
       {
           MoveTo();   
       }

    }


    [ServerRpc(RequireOwnership = false)]
    private void FlipXWalkServerRpc(bool IsWalk)
    {
        NetworkAnimator.Animator.SetBool("IsWalk", IsWalk);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetAttackAnimaServerRpc()
    {
        NetworkAnimator.Animator.SetTrigger("Attack");
    }

    public void ChangeState(state state)
    {
        currentState = state;
    }

    private void CheckIconSelect()
    {
        switch (currentState)
        {
            case state.Select :
                SelectIcon.SetActive(true);
                break;
            case state.UnSelect :
                SelectIcon.SetActive(false);
                break;
        }
    }

    private void MoveTo()
    {
        if (Vector2.Distance(rb.position, targetPosition) > 0.1f)
        {
            //  Animator.SetBool("IsWalk", true);
            FlipXWalkServerRpc(true);
            Vector2 moveDirection = (targetPosition - (Vector2) rb.position).normalized;
            rb.velocity = moveDirection * movementSpeed;
           
            if (moveDirection.x > 0) // Moving right
                SpriteRenderer.flipX = false;
            else if (moveDirection.x < 0) // Moving left
                SpriteRenderer.flipX = true;
        }
        else
        {
            isSetToForceMove = false;
            FlipXWalkServerRpc(false);
            rb.velocity = Vector2.zero;
        }
    }
    
    private void MoveToTargetAndAttack()
    {
        if (TargetToAttack == null)
        {
            return;
        }
        if (TargetToAttack != null)
        {
            targetPosition = TargetToAttack.transform.position;
         
            if (Vector2.Distance(rb.position, targetPosition) > AttackRange)
            {
                MoveTo();
                
            }
            else
            {
                FlipXWalkServerRpc(false);
                rb.velocity = Vector2.zero;
                Attack();
            }

        }
    }
    public void SetTarget(GameObject newTarget)
    {
        TargetToAttack = newTarget;
    }
 
    private void Attack()
    {
        if (attributeUnit.timeSinceLastAttack == 0f)
        {
            SetAttackAnimaServerRpc();
        }

        AttackRange = attributeUnit.AttackRange;
        DetectRange.radius = AttackRange + 1.5f;
        
        attributeUnit.timeSinceLastAttack += Time.deltaTime;
        if (attributeUnit.timeSinceLastAttack >= attributeUnit.AttackCooldown)
        {
           // TargetToAttack.GetComponent<SimpleFlash>().Flash();
           DamageToTargetServerRpc();
            attributeUnit.timeSinceLastAttack = 0f;
        }
       
       
    }

    [ServerRpc]
    private void DamageToTargetServerRpc()
    {
        TargetToAttack.GetComponent<AttributeUnit>().TakeDamage(attributeUnit.Dmg);
      //  DamageToTargetClientRpc();
    }
    
    [ClientRpc]
    private void DamageToTargetClientRpc()
    {
        if(IsOwner) { return; }
        TargetToAttack.GetComponent<AttributeUnit>().TakeDamage(attributeUnit.Dmg);
    }
}

public enum state
{
    Select,
    UnSelect
}

