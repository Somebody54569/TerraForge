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
    private UnitState currentUnitState;
    [SerializeField] private GameObject SelectIcon;
    public float AttackRange;
    public AttributeUnit attributeUnit;
    public List<GameObject> TargetToAttack;
    public GameObject CurrentTarget;
    [SerializeField] private CircleCollider2D DetectRange;
    private bool isSetToForceMove;
    private Vector2 moveDirection;
    public string TestTarget;
    public Color unitColor;
    public SimpleFlash SimpleFlash;

    public List<SpriteRenderer> listToflip;
    //[SerializeField] private SpriteRenderer minimapIconRenderer;
    //[SerializeField] private Color ownerColorOnMap;
    /*public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            minimapIconRenderer.color = ownerColorOnMap;
        }
    }*/

    private void Start()
    {
        TargetToAttack = new List<GameObject>(); 
        TestTarget = "NO";
       ChangeState(state.UnSelect);
       attributeUnit = this.GetComponent<AttributeUnit>();
       AttackRange = attributeUnit.AttackRange;
       DetectRange.radius = AttackRange + 1.5f;
       currentUnitState = UnitState.Idle;
       this.GetComponent<SpriteRenderer>().color = unitColor;
       SimpleFlash.DefulColor = unitColor;
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
                    //SetTarget(null);
                    targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    isSetToForceMove = true;
                    currentUnitState = UnitState.Walk;
                }
            }
            CheckIconSelect();
    }

    private void FixedUpdate()
    { 
        RemoveMissingBuildings();
     
       if (!IsOwner) { return; }
        
    
       if (isSetToForceMove)
       {
           currentUnitState = UnitState.Walk;
           MoveTo();   
       }
       else
       {
           MoveToTargetAndAttack();    
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
        if (currentUnitState == UnitState.Walk)
        {
            if (Vector2.Distance(rb.position, targetPosition) > 0.1f)
            {
                currentUnitState = UnitState.Walk;
                //  Animator.SetBool("IsWalk", true);
                FlipXWalkServerRpc(true); 
                moveDirection = (targetPosition - (Vector2) rb.position).normalized;
                rb.velocity = moveDirection * movementSpeed;
                Flip();

            }
            else
            {
                currentUnitState = UnitState.Idle;
                isSetToForceMove = false;
                FlipXWalkServerRpc(false);
                rb.velocity = Vector2.zero;
            }     
        }
       
    }
    
    private void MoveToTargetAndAttack()
    {
        if (CurrentTarget == null)
        {
            attributeUnit.SetMuzzleServerRpc(false);
            return;
        }
        if (CurrentTarget != null)
        {
            if (Vector2.Distance(rb.position, CurrentTarget.transform.position) < AttackRange)
            {
                FlipXWalkServerRpc(false);
                rb.velocity = Vector2.zero;
                Attack();
            }
           

        }
    }
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
 
    private void Attack()
    {
        if (attributeUnit.timeSinceLastAttack == 0f)
        {
            AudioManager.Instance.PlaySFX("Attack");
            attributeUnit.SetMuzzleServerRpc(false);
            SetAttackAnimaServerRpc();
        }

        AttackRange = attributeUnit.AttackRange;
        DetectRange.radius = AttackRange + 1.5f;
        
        attributeUnit.timeSinceLastAttack += Time.deltaTime;
        if (attributeUnit.timeSinceLastAttack >= attributeUnit.AttackCooldown)
        {
            DamageToTargetServerRpc();
            CurrentTarget.GetComponent<AttributeUnit>().Flash();
            attributeUnit.SetMuzzleServerRpc(true);
            attributeUnit.timeSinceLastAttack = 0f;
        }
       
    }


 /*   [ServerRpc(RequireOwnership = false)]
    private void FlashServerRpc()
    {
        CurrentTarget.GetComponent<SimpleFlash>().Flash();
        FlashClientRpc();
    }
    [ClientRpc]
    private void FlashClientRpc()
    {
        if (IsHost)
        {
            return;
        }
        CurrentTarget.GetComponent<SimpleFlash>().Flash();
    }

  */
    [ServerRpc]
    private void DamageToTargetServerRpc()
    {
        CurrentTarget.GetComponent<AttributeUnit>().TakeDamage(attributeUnit.Dmg);
 //      FlashServerRpc();
      //  DamageToTargetClientRpc();
    }
    [ClientRpc]
    private void DamageToTargetClientRpc()
    {
        if (IsHost)
        {
            return;
        }
        CurrentTarget.GetComponent<AttributeUnit>().TakeDamage(attributeUnit.Dmg);
    }

    #region Flip
    private void Flip()
    {
        // Walk to the right
        if (moveDirection.x > 0)
        {
            foreach (SpriteRenderer sprite in listToflip)
            {
                sprite.flipX = false;
            }
            
            FlipServerRpc(SpriteRenderer.flipX);
        }
        // Walk to the left
        else if (moveDirection.x < 0)
        {
            foreach (SpriteRenderer sprite in listToflip)
            {
                sprite.flipX = true;
            }
            FlipServerRpc(SpriteRenderer.flipX);
        }
    }

    [ServerRpc]
    private void FlipServerRpc(bool flipX)
    {
        FlipClientRpc(flipX);
    }

    [ClientRpc]
    private void FlipClientRpc(bool flipX)
    {
        foreach (SpriteRenderer sprite in listToflip)
        {
            sprite.flipX = flipX;
        }
    }
    #endregion
    
}

public enum state
{
    Select,
    UnSelect
}

public enum UnitState
{
    Idle,
    Walk,
}

