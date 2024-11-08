using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Struct to hold resource requirements for building or upgrading
[Serializable]
public struct RessourceRequirement
{
    public string resourceType;
    public int quantity;
}

public class VillageManager : MonoBehaviour
{
    //List of villagers in the village
    public List<HumanVillageInfos> villagers = new List<HumanVillageInfos>();
    private bool villageInitialized = false;    

    //Village level data for different stages
    [SerializeField] private VillageLevelData[] villageLevelDatas = new VillageLevelData[5];
    private VillageLevelData currentVillageLevelData;

    //House level data and prefab
    [SerializeField] private HouseLevelData[] houseLevelDatas;
    [SerializeField] private GameObject housePrefab;
    private List<GameObject> houses = new List<GameObject>();
    private int currentLevel = 1;

    private VillageStorage villageStorage;
    private SpriteRenderer spriteRenderer;
    private List<Vector3> occupiedPositions = new List<Vector3>();
    private bool everythingMaxLevelInVillage = false;

    private TimeManager timeManager;

    private void Start()
    {
        // Initialize village storage and level data
        villageStorage = GetComponent<VillageStorage>();
        currentVillageLevelData = villageLevelDatas[0];
        villageStorage.SetMaxStorageValues(currentVillageLevelData);

        spriteRenderer = GetComponent<SpriteRenderer>();

        timeManager = GameObject.FindWithTag("Managers").GetComponent<TimeManager>();

        // Start house construction management
        //InvokeRepeating("HandleHouseConstruction", 5f, timeManager.monthDuration / timeManager.timeSpeed);
        StartCoroutine(HandleHouseConstruction());
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
        if(villageLevelDatas[currentLevel - 1] != villageLevelDatas[villageLevelDatas.Length - 1])
        {
            foreach (var requirement in currentVillageLevelData.ressourcesNeededForNextLevel)
            {
                if (villageStorage.GetResourceAmount(requirement.resourceType) < requirement.quantity)
                {
                    Debug.Log("Not enough resources to upgrade the village.");
                    return;
                }
                else
                {
                    foreach (var ressources in currentVillageLevelData.ressourcesNeededForNextLevel)
                    {
                        villageStorage.RemoveResource(requirement.resourceType, requirement.quantity);
                    }

                    //Upgrade village level/ update storage values/ change sprite
                    if (currentLevel < villageLevelDatas.Length)
                    {
                        currentLevel++;
                        currentVillageLevelData = villageLevelDatas[currentLevel - 1];
                        spriteRenderer.sprite = currentVillageLevelData.sprite;
                        villageStorage.SetMaxStorageValues(currentVillageLevelData);
                        everythingMaxLevelInVillage = false;

                        //update villagers stats
                        foreach (var human in villagers)
                        {
                            HumanGetStats humanGetStats = human.GetComponent<HumanGetStats>();
                            humanGetStats.SetNewStats(currentVillageLevelData.villagersStats);
                        }
                    }
                }
            }
        }        
    }

    public int GetVillagerCount()
    {
        return villagers.Count;
    }

    // Designate a new chief for the village
    public void DesignateNewChief()
    {
        if (villagers.Count > 0)
        {
            HumanVillageInfos newChief = villagers[1]; //Choose a villager as chief
            newChief.isVillageChief = true;
            Debug.Log(newChief.name + " is the new village chief.");
        }
    }

    IEnumerator HandleHouseConstruction()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeManager.monthDuration / timeManager.timeSpeed);

            //Check if we need to build or upgrade houses
            if (houses.Count < currentVillageLevelData.maxHouses)
            {
                BuildHouse();
            }
            else
            {
                UpgradeHouses();
            }

            //If all houses are built and at max level, upgrade the village
            if (houses.Count == currentVillageLevelData.maxHouses && everythingMaxLevelInVillage)
            {
                UpgradeVillage();
            }
        }        
    }

    private void BuildHouse()
    {
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

        //Check for overlaps with existing houses
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
        for (int i = 0; i < houses.Count; i++)
        {
            HouseManager currentHouseManager = houses[i].GetComponent<HouseManager>();
            int currentHouseLevel = currentHouseManager.GetCurrentLevel();

            //Check if the house can be upgraded according to village level
            if (currentHouseLevel < currentVillageLevelData.maxHousesLevel)
            {
                HouseLevelData nextLevelData = houseLevelDatas[currentHouseLevel];

                //Check if the resources needed are available
                if (CanBuildHouse(nextLevelData))
                {
                    foreach (var requirement in nextLevelData.ressourcesNeededToBuild)
                    {
                        villageStorage.RemoveResource(requirement.resourceType, requirement.quantity);
                    }

                    currentHouseManager.Upgrade(nextLevelData.sprite);

                    return;
                }
            }
        }

        //If all houses are at max level, set a bool to true to allow village to upgrade
        if (houses.TrueForAll(h => h.GetComponent<HouseManager>().GetCurrentLevel() >= currentVillageLevelData.maxHousesLevel))
        {
            everythingMaxLevelInVillage = true;
        }
    }

    private bool CanBuildHouse(HouseLevelData houseLevelData)
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

    public void DestroyVillage()
    {
        for (int i = houses.Count - 1; i >= 0; i--)
        {
            if (houses[i] != null)
            {
                Destroy(houses[i]);
            }
        }

        if(villagers.Count > 0)
        {
            foreach(var villager in villagers)
            {
                Destroy(villager.gameObject);
            }
        }

        Destroy(gameObject);
    }

    public HumansStats GetVillagersStats()
    {
        return currentVillageLevelData.villagersStats;
    }

    public VillageStorage GetVillageStorage()
    {
        return gameObject.GetComponent<VillageStorage>();
    }
}
