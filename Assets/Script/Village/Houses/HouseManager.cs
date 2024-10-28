using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
    public int currentLevel = 1;
    public SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Upgrade(Sprite newSprite)
    {
        currentLevel++;
        spriteRenderer.sprite = newSprite;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }
}
