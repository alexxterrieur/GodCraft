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
            AddRessources("wood", 10);
            Debug.Log("test level storage in VillageStorage");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            AddRessources("fruit", 10);
            Debug.Log("test level storage in VillageStorage");
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
                    Debug.Log("StorageFull -> wood");
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
