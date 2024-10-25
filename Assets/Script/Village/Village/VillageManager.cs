using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageManager : MonoBehaviour
{
    public List<HumanVillageInfos> villagers = new List<HumanVillageInfos>();
    private bool villageInitialized = false;

    private int currentLevel = 1;
    [SerializeField] private VillageLevelData[] villageLevelDatas = new VillageLevelData[5];
    private VillageLevelData currentVillageLevelData;

    private VillageStorage villageStorage;

    private void Start()
    {
        villageStorage = GetComponent<VillageStorage>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("test level evolution in VillageManager");
            EvolveVillage();
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

            currentVillageLevelData = villageLevelDatas[0];
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
        if(currentLevel < villageLevelDatas.Length)
        {
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
            HumanVillageInfos newChief = villagers[1]; //Index 1 because call before removing current the chief
            newChief.isVillageChief = true;
            Debug.Log(newChief.name + " is the new village chief.");
        }
    }
}
