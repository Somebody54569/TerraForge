using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Attribute = System.Attribute;

public class UnitBehevior : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float movementSpeed = 4f;
    private Vector2 targetPosition;
    public SpriteRenderer SpriteRenderer;
    public Animator Animator;
    public NetworkAnimator NetworkAnimator;
    private state currentState;
    [SerializeField] private GameObject SelectIcon;

    private void Start()
    {
       ChangeState(state.UnSelect);
    }
    

    private void Update()
    {
        if (!IsOwner) { return; }
            SelectIcon.SetActive(false);
            if (Input.GetMouseButtonDown(1))
            {
                if (currentState == state.Select)
                {
                    targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
            }
            CheckIconSelect();
    }

    private void FixedUpdate()
    {
       if (!IsOwner) { return; }
       
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
            FlipXWalkServerRpc(false);
            rb.velocity = Vector2.zero;
        }
    }

    [ServerRpc]
    private void FlipXWalkServerRpc(bool IsWalk)
    {
        NetworkAnimator.Animator.SetBool("IsWalk", IsWalk);
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
    
}

public enum state
{
    Select,
    UnSelect
}

