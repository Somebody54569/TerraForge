using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UnitSelection : MonoBehaviour
{
    public RectTransform selectionBoxUI;
    private Vector2 boxStartPosition;
    private Vector2 boxEndPosition;
    public LayerMask selectableLayer;
    public PlayerManager PlayerManager;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
        }
    }
    void SelectObjectsInBox()
    {
        AllUnSelect();
        Rect selectionRect = new Rect(boxStartPosition.x, boxStartPosition.y,
            boxEndPosition.x - boxStartPosition.x,
            boxEndPosition.y - boxStartPosition.y);

        Collider2D[] hitColliders =  Physics2D.OverlapAreaAll(boxStartPosition, boxEndPosition);

        foreach (Collider2D collider in hitColliders)
        {
            GameObject selectableObject = collider.gameObject;

            // Check if the object is in the selectable layer
            if (((1 << selectableObject.layer) & selectableLayer) != 0)
            {
                    if (selectableObject.CompareTag("Unit"))
                {
                    PlayerManager.SelectUnit.Add(selectableObject.GetComponent<UnitBehevior>());
                    selectableObject.GetComponent<UnitBehevior>().ChangeState(state.Select);
                }
            }
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
