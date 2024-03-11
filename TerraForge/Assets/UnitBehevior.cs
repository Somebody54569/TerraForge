using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class UnitBehevior : NetworkBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float movementSpeed = 4f;
    private Vector2 targetPosition;
    public SpriteRenderer SpriteRenderer;
    public Animator Animator;
    public NetworkAnimator NetworkAnimator;
    private void Update()
    {
        if (!IsOwner) { return; }
        
            if (Input.GetMouseButtonDown(1))
            {
                targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
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
}
