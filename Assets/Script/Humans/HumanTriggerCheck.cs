using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanTriggerCheck : MonoBehaviour
{
    private int humansInTrigger = 1;
    [SerializeField] private int numberOfHumansNeededToCreateVillage;
    [SerializeField] private GameObject villageMainBuildingPrefab;
    private List<HumanVillageInfos> humansInRange = new List<HumanVillageInfos>();
    private bool villageCreated = false;
    private HumanVillageInfos selfHumanManager;
    private VillageManager nearbyVillageManager;

    private void Start()
    {
        selfHumanManager = GetComponent<HumanVillageInfos>();

        if (selfHumanManager != null && !selfHumanManager.belongsToVillage)
        {
            humansInRange.Add(selfHumanManager);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HumanVillageInfos human = collision.gameObject.GetComponent<HumanVillageInfos>();

        if (human != null && !human.belongsToVillage && !humansInRange.Contains(human))
        {
            if (IsNearbyVillage(out nearbyVillageManager))
            {
                nearbyVillageManager.AddVillager(human);
            }
            else
            {
                humansInRange.Add(human);
                humansInTrigger++;

                if (!villageCreated && humansInTrigger >= numberOfHumansNeededToCreateVillage)
                {
                    CreateVillage();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        HumanVillageInfos human = collision.gameObject.GetComponent<HumanVillageInfos>();

        if (human != null && humansInRange.Contains(human))
        {
            humansInRange.Remove(human);
            humansInTrigger--;
        }
    }

    private void CreateVillage()
    {
        if (villageCreated || humansInRange.Count < numberOfHumansNeededToCreateVillage) return;

        HumanVillageInfos villageChief = humansInRange[0];
        villageChief.isVillageChief = true;

        GameObject villageMainBuilding = Instantiate(villageMainBuildingPrefab, villageChief.transform.position, Quaternion.identity);
        VillageManager villageManager = villageMainBuilding.GetComponent<VillageManager>();

        villageManager.InitializeVillage(humansInRange);

        villageCreated = true;
    }

    private bool IsNearbyVillage(out VillageManager existingVillageManager)
    {
        existingVillageManager = null;

        foreach (HumanVillageInfos human in humansInRange)
        {
            if (human.belongsToVillage)
            {
                existingVillageManager = human.village;
                return true;
            }
        }

        return false;
    }
}
