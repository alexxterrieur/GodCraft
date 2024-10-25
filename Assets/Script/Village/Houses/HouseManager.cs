using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
    private int currentLevel = 1;

    public void Upgrade()
    {
        currentLevel++;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }
}
