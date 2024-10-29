using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageStorage : MonoBehaviour
{
    public int currentWoods;
    public int currentStones;
    public int currentOres;
    public int currentMeats;
    public int currentFruits;
    public int currentWater;


    private int maxWoods;
    private int maxStones;
    private int maxOres;
    private int maxMeats;
    private int maxFruits;
    private int maxWater;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddRessources("wood", 5);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            AddRessources("stone", 5);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            AddRessources("ore", 5);
        }
    }

    public void SetMaxStorageValues(VillageLevelData villageLevel)
    {
        maxWoods = villageLevel.maxWoods;
        maxStones = villageLevel.maxStones;
        maxOres = villageLevel.maxOres;
        maxMeats = villageLevel.maxMeats;
        maxFruits = villageLevel.maxFruits;
        maxWater = villageLevel.maxWater;
    }

    public int GetResourceAmount(string resourceType)
    {
        switch (resourceType)
        {
            case "wood": return currentWoods;
            case "stone": return currentStones;
            case "ore": return currentOres;
            case "meat": return currentMeats;
            case "fruit": return currentFruits;
            case "water": return currentWater;
            default:
                Debug.Log("Unknown resource type: " + resourceType);
                return 0;
        }
    }

    public void RemoveResource(string resourceType, int amount)
    {
        switch (resourceType)
        {
            case "wood": currentWoods = Mathf.Max(0, currentWoods - amount); break;
            case "stone": currentStones = Mathf.Max(0, currentStones - amount); break;
            case "ore": currentOres = Mathf.Max(0, currentOres - amount); break;
            case "meat": currentMeats = Mathf.Max(0, currentMeats - amount); break;
            case "fruit": currentFruits = Mathf.Max(0, currentFruits - amount); break;
            case "water": currentWater = Mathf.Max(0, currentWater - amount); break;
        }
    }

    public void AddRessources(string ressourceType, int amount)
    {
        switch (ressourceType)
        {
            case ("wood"):
                if (currentWoods + amount < maxWoods)
                {
                    currentWoods += amount;
                }
                else
                    Debug.Log("StorageFull -> wood" + maxWoods);
                break;

            case ("stone"):
                if (currentStones + amount < maxStones)
                {
                    currentStones += amount;
                }
                else
                    Debug.Log("StorageFull -> stone");
                break;

            case ("ore"):
                if (currentOres + amount < maxOres)
                {
                    currentOres += amount;
                }
                else
                    Debug.Log("StorageFull -> ores");
                break;

            case ("meat"):
                if (currentMeats + amount < maxMeats)
                {
                    currentMeats += amount;
                }
                else
                    Debug.Log("StorageFull -> meat");
                break;

            case ("fruit"):
                if (currentFruits + amount < maxFruits)
                {
                    currentFruits += amount;
                }
                else
                    Debug.Log("StorageFull -> fruit");
                break;

            case ("water"):
                if(currentWater +  amount < maxWater)
                {
                    currentWater += amount;
                }
                else
                    Debug.Log("StorageFull -> water");
                break;
        }
    }
}
