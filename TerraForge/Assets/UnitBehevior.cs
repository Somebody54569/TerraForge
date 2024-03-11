using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitBehevior : NetworkBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float movementSpeed = 4f;
    private Vector2 targetPosition;

    public Animator Animator;
    
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
            Animator.SetBool("IsWalk", true);
            Vector2 moveDirection = (targetPosition - (Vector2)rb.position).normalized;
            rb.velocity = moveDirection * movementSpeed;
        }
        else
        {
            Animator.SetBool("IsWalk", false);
            rb.velocity = Vector2.zero;
        }
    }
}
