using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    private SpriteRenderer spriteRenderer;


    private List<Vector3> occupiedPositions = new List<Vector3>();

    private void Start()
    {
        villageStorage = GetComponent<VillageStorage>();
        currentVillageLevelData = villageLevelDatas[0];
        villageStorage.SetMaxStorageValues(currentVillageLevelData);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            UpgradeVillage();
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

    public void UpgradeVillage()
    {
        if (currentLevel < villageLevelDatas.Length)
        {
            foreach (var requirement in currentVillageLevelData.ressourcesNeededForNextLevel)
            {
                villageStorage.RemoveResource(requirement.resourceType, requirement.quantity);
            }

            currentLevel++;
            currentVillageLevelData = villageLevelDatas[currentLevel - 1];
            spriteRenderer.sprite = currentVillageLevelData.sprite;
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
        Vector3 randomPosition = Vector3.zero;

        float houseSize = 4f;

        for (int i = 0; i < 10; i++)
        {
            randomPosition = transform.position + new Vector3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), 0);

            if (IsPositionValid(randomPosition, houseSize))
            {
                return randomPosition;
            }
        }

        return transform.position;
    }

    private bool IsPositionValid(Vector3 position, float houseSize)
    {
        if (position.x < transform.position.x - 20 || position.x > transform.position.x + 20 || position.y < transform.position.y - 20 || position.y > transform.position.y + 20) //modifier avec l'update de la taille des villages 
        {
            return false;
        }

        foreach (GameObject house in houses)
        {
            Vector3 housePosition = house.transform.position;

            if (Mathf.Abs(housePosition.x - position.x) < houseSize && Mathf.Abs(housePosition.y - position.y) < houseSize)
            {
                return false;
            }
        }

        return true;
    }

    private void UpgradeHouses()
    {
        bool allHousesAtMaxLevel = true;

        for (int i = houses.Count - 1; i >= 0; i--)
        {
            HouseManager currentHouseManager = houses[i].GetComponent<HouseManager>();
            int currentHouseLevel = currentHouseManager.GetCurrentLevel();

            if (currentHouseLevel < currentVillageLevelData.maxHousesLevel)
            {
                HouseLevelData nextLevelData = houseLevelDatas[currentHouseLevel];

                if (CanBuildHouse(nextLevelData))
                {
                    foreach (var requirement in nextLevelData.ressourcesNeededToBuild)
                    {
                        villageStorage.RemoveResource(requirement.resourceType, requirement.quantity);
                    }

                    currentHouseManager.Upgrade(nextLevelData.sprite);

                    Debug.Log("House upgraded to level: " + (currentHouseLevel + 1));

                    allHousesAtMaxLevel = false;
                    break;
                }
                else
                {
                    Debug.Log("Not enough resources to upgrade this house further.");
                    allHousesAtMaxLevel = false;
                    break;
                }
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