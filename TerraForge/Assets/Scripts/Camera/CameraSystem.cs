using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    
    [SerializeField] float moveSpeed;
    [SerializeField] private float edgeScrollingSpeed;

    [SerializeField] private bool useEdgeScrolling;
    [SerializeField] private bool useDragPan;
    
    [SerializeField] private int edgeScrollSize = 10; 

    [SerializeField] float dragPanSpeed = 10f;
    private Vector3 dragOrigin;
    private bool dragPanMoveActive = false;
    private Vector2 lastMousePosition;

    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float FOVMax = 10;
    [SerializeField] private float FOVMin = 1;
    private float targetFOV = 5f;
    
    // Update is called once per frame
    
    void Update()
    {
        HandleCameraMovement();
        HandleCameraZoom();
        
        if (useEdgeScrolling)
        {
            HandleCameraMovementEdgeScrolling();
        }

        if (useDragPan)
        {
            HandleCameraDragPan();
        }
        
    }

    private void HandleCameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);
        
        if (Input.GetKey(KeyCode.W)) inputDir.y = +1f;
        if (Input.GetKey(KeyCode.S)) inputDir.y = -1f;
        if (Input.GetKey(KeyCode.A)) inputDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputDir.x = +1f;
        
        transform.position += inputDir * moveSpeed * Time.deltaTime;
    }

    private void HandleCameraMovementEdgeScrolling()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);
        
        if (Input.mousePosition.x < edgeScrollSize)
        {
            inputDir.x = -1f;
        }
        if (Input.mousePosition.y < edgeScrollSize)
        {
            inputDir.y = -1f;
        }
        if (Input.mousePosition.x > Screen.width - edgeScrollSize)
        {
            inputDir.x = +1f;
        }
        if (Input.mousePosition.y > Screen.height - edgeScrollSize)
        {
            inputDir.y = +1f;
        }
        
        transform.position += inputDir * edgeScrollingSpeed * Time.deltaTime;
    }
    
    private void HandleCameraDragPan()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);
        if (Input.GetMouseButtonDown(2))
        {
            dragPanMoveActive = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(2))
        {
            dragPanMoveActive = false;
        }

        if (dragPanMoveActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;
            
            inputDir.x =  mouseMovementDelta.x ;
            inputDir.y =  mouseMovementDelta.y ;
            
            lastMousePosition = Input.mousePosition;
        }
        
        transform.position += inputDir * dragPanSpeed * Time.deltaTime;
    }
    
    private void HandleCameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            targetFOV += 1;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFOV -= 1;
        }

        targetFOV = Mathf.Clamp(targetFOV, FOVMin, FOVMax);
        
        cinemachineVirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(cinemachineVirtualCamera.m_Lens.OrthographicSize, targetFOV, Time.deltaTime * zoomSpeed);
    }
    
}
