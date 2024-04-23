using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.Netcode;

public class UnitSelection : NetworkBehaviour
{
    public RectTransform selectionBoxUI;
    private Vector2 boxStartPosition;
    private Vector2 boxEndPosition;
    public LayerMask selectableLayer;
    public PlayerManager PlayerManager;
    private GameObject TargetTemp;
    private List<UnitBehevior> tempUnitBeheviors;

    void Start()
    {
        tempUnitBeheviors = new List<UnitBehevior>();
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        if (!IsMouseOverButton())
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                boxStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                selectionBoxUI.gameObject.SetActive(true);
                selectionBoxUI.position = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                boxEndPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                SelectObjectsInBox();
                selectionBoxUI.gameObject.SetActive(false);

            }

            if (Input.GetMouseButton(0))
            {
                if (selectionBoxUI.gameObject.activeInHierarchy)
                {
                    Vector2 currentMousePosition = Input.mousePosition;
                    Vector2 sizeDelta = currentMousePosition - (Vector2) selectionBoxUI.position;

                    // Flip the UI image if dragging from right to left or bottom to top
                    if (sizeDelta.x < 0)
                    {
                        sizeDelta.x = -sizeDelta.x;
                        selectionBoxUI.pivot = new Vector2(1f, selectionBoxUI.pivot.y);
                    }
                    else
                    {
                        selectionBoxUI.pivot = new Vector2(0f, selectionBoxUI.pivot.y);
                    }

                    if (sizeDelta.y < 0)
                    {
                        sizeDelta.y = -sizeDelta.y;
                        selectionBoxUI.pivot = new Vector2(selectionBoxUI.pivot.x, 1f);
                    }
                    else
                    {
                        selectionBoxUI.pivot = new Vector2(selectionBoxUI.pivot.x, 0f);
                    }

                    selectionBoxUI.sizeDelta = sizeDelta;
                }
            }

            if (Input.GetMouseButton(1))
            {
                if (PlayerManager.SelectUnit.Count > 0)
                {
                  //  SetTargetByRay();
                }
            }
           
        }
    }
    private void SelectObjectsInBox()
    {
        AllUnSelect();
        tempUnitBeheviors.Clear();
        Collider2D[] hitColliders =  Physics2D.OverlapAreaAll(boxStartPosition, boxEndPosition);

        foreach (Collider2D collider in hitColliders)
        {
            GameObject selectableObject = collider.gameObject;

            // Check if the object is in the selectable layer
            if (((1 << selectableObject.layer) & selectableLayer) != 0)
            {
                if (selectableObject.CompareTag("Unit"))
                {
                    if (selectableObject.GetComponent<UnitBehevior>().IsOwner)
                    {
                        PlayerManager.SelectUnit.Add(selectableObject.GetComponent<UnitBehevior>());
                        tempUnitBeheviors.Add(selectableObject.GetComponent<UnitBehevior>());
                        selectableObject.GetComponent<UnitBehevior>().ChangeState(state.Select);            
                    }
                }
            }
        }
    }
    
    public void SetTargetByRay()
    {
        RaycastHit2D hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        if (hit.collider != null && hit.collider.CompareTag("Unit"))
        {
            UnitBehevior unitBehevior =  hit.collider.GetComponent<UnitBehevior>();
            if (!unitBehevior.IsOwner)
            {
                TargetTemp = hit.collider.gameObject;
            }
        }
        if (hit.collider != null && hit.collider.CompareTag("Base"))
        {
            Building building =  hit.collider.GetComponent<Building>();
            if (!building.IsOwner)
            {
                TargetTemp = hit.collider.gameObject;
            }
        }

        if (TargetTemp != null)
        {
            SetTargetByRayServerRpc();      
        }
      
        
    }
    [ServerRpc]
    public void SetTargetByRayServerRpc()
    {
        foreach (var VARIABLE in PlayerManager.SelectUnit)
        {
            VARIABLE.SetTarget(TargetTemp);
            Debug.Log(VARIABLE.name);
            VARIABLE.TestTarget = TargetTemp.name;
        }
        SetTargetByRayClientRpc();
        
    }
    [ClientRpc]
    public void SetTargetByRayClientRpc()
    {
        if (IsOwner)
        {
            return;
        }
        foreach (var VARIABLE in PlayerManager.SelectUnit)
        {
            VARIABLE.SetTarget(TargetTemp);
            Debug.Log(VARIABLE.name);
            VARIABLE.TestTarget = TargetTemp.name;
        }
    }
    
    public void AllUnSelect()
    {
        foreach (var VARIABLE in  PlayerManager.SelectUnit)
        {
            VARIABLE.ChangeState(state.UnSelect);
        }
        
        PlayerManager.SelectUnit.Clear();
    }
    private bool IsMouseOverButton()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Button>() != null)
            {
                return true;
            }
        }

        return false;
    }
    private bool IsMouseOverPanel()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            int layer = LayerMask.NameToLayer("UIPanel");
            if (result.gameObject.layer == layer)
            {
                return true;
            }
        }

        return false;
    }
}
