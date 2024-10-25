using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RessourceRequirement
{
    public string resourceType;
    public int quantity;
}


public class VillageManager : MonoBehaviour
{
    public List<HumanVillageInfos> villagers = new List<HumanVillageInfos>();
    private bool villageInitialized = false;

    private int currentLevel = 1;
    [SerializeField] private VillageLevelData[] villageLevelDatas = new VillageLevelData[5];
    private VillageLevelData currentVillageLevelData;

    [SerializeField] private HouseLevelData[] houseLevelDatas;
    [SerializeField] private GameObject housePrefab;
    private List<GameObject> houses = new List<GameObject>();

    private VillageStorage villageStorage;

    private void Start()
    {
        villageStorage = GetComponent<VillageStorage>();
        currentVillageLevelData = villageLevelDatas[0];
        villageStorage.SetMaxStorageValues(currentVillageLevelData);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            EvolveVillage();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            HandleHouseConstruction();

        }
    }

    public void InitializeVillage(List<HumanVillageInfos> initialVillagers)
    {
        if (!villageInitialized)
        {
            foreach (var human in initialVillagers)
            {
                AddVillager(human);
            }

            villageInitialized = true;
        }
    }

    public void AddVillager(HumanVillageInfos human)
    {
        if (!villagers.Contains(human))
        {
            villagers.Add(human);
            human.belongsToVillage = true;
            human.village = this;
        }
    }

    public void RemoveVillager(HumanVillageInfos human)
    {
        if (villagers.Contains(human))
        {
            villagers.Remove(human);
            human.belongsToVillage = false;
            human.village = null;
        }
    }

    public void EvolveVillage()
    {
        if (currentLevel < villageLevelDatas.Length)
        {
            foreach (var requirement in currentVillageLevelData.ressourcesNeededForNextLevel)
            {
                villageStorage.RemoveResource(requirement.resourceType, requirement.quantity);
            }

            currentLevel++;
            currentVillageLevelData = villageLevelDatas[currentLevel - 1];
            villageStorage.SetMaxStorageValues(currentVillageLevelData);
        }
    }

    public int GetVillagerCount()
    {
        return villagers.Count;
    }

    public void DesignateNewChief()
    {
        if (villagers.Count > 0)
        {
            HumanVillageInfos newChief = villagers[1];
            newChief.isVillageChief = true;
            Debug.Log(newChief.name + " is the new village chief.");
        }
    }

    private void HandleHouseConstruction()
    {
        //Check if we need to build or upgrade houses
        if (houses.Count < currentVillageLevelData.maxHouses)
        {
            BuildHouse();
        }
        else
        {
            UpgradeHouses();
        }
    }

    private void BuildHouse()
    {
        //Get data for the base house level
        HouseLevelData houseLevelData = houseLevelDatas[0];

        //Check if we have enough resources to build a new house
        if (CanBuildHouse(houseLevelData))
        {
            foreach (var requirement in houseLevelData.ressourcesNeededToBuild)
            {
                villageStorage.RemoveResource(requirement.resourceType, requirement.quantity);
            }

            Vector3 randomPosition = GetRandomPositionAroundVillage();
            GameObject newHouse = Instantiate(housePrefab, randomPosition, Quaternion.identity);
            houses.Add(newHouse);
        }
    }

    private Vector3 GetRandomPositionAroundVillage()
    {
        return transform.position;
    }

    private void UpgradeHouses()
    {
        bool allHousesAtMaxLevel = true;

        for (int i = 0; i < houses.Count; i++)
        {
            int currentHouseLevel = houses[i].GetComponent<HouseManager>().GetCurrentLevel();

            if (currentHouseLevel < currentVillageLevelData.maxHousesLevel)
            {
                HouseLevelData nextLevelData = houseLevelDatas[currentHouseLevel];

                if (CanBuildHouse(nextLevelData))
                {
                    foreach (var requirement in nextLevelData.ressourcesNeededToBuild)
                    {
                        villageStorage.RemoveResource(requirement.resourceType, requirement.quantity);
                    }

                    houses[i].GetComponent<HouseManager>().Upgrade();
                    Debug.Log("House upgraded to level: " + (currentHouseLevel + 1));
                }

                allHousesAtMaxLevel = false;
            }
        }

        if (allHousesAtMaxLevel)
        {
            Debug.Log("le village doit evoluer");
        }
    }

    public bool CanBuildHouse(HouseLevelData houseLevelData)
    {
        foreach (var requirement in houseLevelData.ressourcesNeededToBuild)
        {
            int currentAmount = villageStorage.GetResourceAmount(requirement.resourceType);
            if (currentAmount < requirement.quantity)
            {
                Debug.Log("Not enough " + requirement.resourceType + " to build the house.");
                return false;
            }
        }
        return true;
    }
}
