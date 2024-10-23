using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageManager : MonoBehaviour
{
    public List<HumanVillageInfos> villagers = new List<HumanVillageInfos>();
    private bool villageInitialized = false;

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

    public int GetVillagerCount()
    {
        return villagers.Count;
    }

    public void DesignateNewChief()
    {
        if (villagers.Count > 0)
        {
            HumanVillageInfos newChief = villagers[0];
            newChief.isVillageChief = true;
            Debug.Log("newChief");
        }
    }
}
