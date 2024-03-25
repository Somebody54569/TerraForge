using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public struct SelectionButton
{
    public Image colorButton;
    public GameObject selectionBox;
    public Color color;
}
public class ColorSelector : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] playerSprites;
    [SerializeField] private SelectionButton[] selectionButton;
    [SerializeField] private int colorIndex = 0;

    public const string PlayerColorKey = "PlayerColorIndex";

    private void OnValidate()
    {
        foreach (var selection in selectionButton)
        {
            selection.colorButton.color = selection.color;
        }
    }

    private void Start()
    {
        colorIndex = PlayerPrefs.GetInt(PlayerColorKey, 0);
        HandleColorChanged();
    }

    private void HandleColorChanged()
    {
        foreach (var selection in selectionButton)
        {
            selection.selectionBox.SetActive(false);
        }

        foreach (var sprite in playerSprites)
        {
            sprite.color = selectionButton[colorIndex].color;
        }
        selectionButton[colorIndex].selectionBox.SetActive(true);
    }

    public void SelectColor(int colorIndex)
    {
        this.colorIndex = colorIndex;
        HandleColorChanged();
    }

    public void SavColor()
    {
        PlayerPrefs.SetInt(PlayerColorKey, colorIndex);
    }
}
