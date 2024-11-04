using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInventory : MonoBehaviour
{
    public int currentWoods;
    public int currentStones;
    public int currentOres;
    public int currentFruits;
    public int currentMeats;
    public int currentWaterStack;

    private int maxWoods = 70;
    private int maxStones = 70;
    private int maxOres = 30;
    private int maxMeats = 15;
    private int maxFruits = 15;
    private int maxWater = 15;

    public bool isFullOfSomething = false;

    public void AddRessource(string resourceType, int amount)
    {
        switch (resourceType)
        {
            case ("wood"):
                if (currentWoods + amount <= maxWoods)
                {
                    currentWoods += amount;
                }
                else
                {
                    currentWoods = maxWoods;
                    isFullOfSomething = true;
                }
                break;

            case ("stone"):
                if (currentStones + amount <= maxStones)
                {
                    currentStones += amount;
                }
                else
                {
                    currentStones = maxStones;
                    isFullOfSomething = true;
                }
                break;

            case ("ore"):
                if (currentOres + amount <= maxOres)
                {
                    currentOres += amount;
                }
                else
                {
                    currentOres = maxOres;
                    isFullOfSomething = true;
                }
                break;

            case ("meat"):
                if (currentMeats + amount <= maxMeats)
                {
                    currentMeats += amount;
                }
                else
                {
                    currentMeats = maxMeats;
                    isFullOfSomething = true;
                }
                break;

            case ("fruit"):
                if (currentFruits + amount <= maxFruits)
                {
                    currentFruits += amount;
                }
                else
                {
                    currentFruits = maxFruits;
                    isFullOfSomething = true;
                }
                break;

            case ("water"):
                if (currentWaterStack + amount <= maxWater)
                {
                    currentWaterStack += amount;
                }
                else
                {
                    currentWaterStack = maxWater;
                    isFullOfSomething = true;
                }
                break;
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
            case "water": currentWaterStack = Mathf.Max(0, currentWaterStack - amount); break;
        }
    }

    public void TransferToVillageStorage(VillageStorage villageStorage)
    {
        //Add current ressources to village storage
        villageStorage.AddRessources("wood", currentWoods);
        villageStorage.AddRessources("stone", currentStones);
        villageStorage.AddRessources("ore", currentOres);
        villageStorage.AddRessources("meat", currentMeats);
        villageStorage.AddRessources("fruit", currentFruits);
        villageStorage.AddRessources("water", currentWaterStack);

        //Reset ressources
        RemoveResource("wood", currentWoods);
        RemoveResource("stone", currentStones);
        RemoveResource("ore", currentOres);
        RemoveResource("meat", currentMeats);
        RemoveResource("fruit", currentFruits);
        RemoveResource("water", currentWaterStack);

        isFullOfSomething = false;
    }
}
