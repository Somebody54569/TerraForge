using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [SerializeField] private PlayerManager player;
    [SerializeField] private SpriteRenderer[] playerSprites;
    public Color[] playerColor;
    public int colorIndex;

    private void Start()
    {
        HandlePlayerColorChanged(0, player.PlayerColorIndex.Value);
        player.PlayerColorIndex.OnValueChanged += HandlePlayerColorChanged;
    }

    private void HandlePlayerColorChanged(int oldIndex, int newIndex)
    {
        colorIndex = newIndex;
        foreach (var sprite in playerSprites)
        {
            sprite.color = playerColor[colorIndex];
        }
    }

    private void OnDestroy()
    {
        player.PlayerColorIndex.OnValueChanged -= HandlePlayerColorChanged;
    }
}
