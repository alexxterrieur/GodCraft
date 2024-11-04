using UnityEngine;
using System.Collections;

public class ResourceParameters : MonoBehaviour
{
    public string resourceName;
    public int resourceAmount;
    public float harvestTime = 5f;
    public bool canRespawn = false;
    [SerializeField] private float resourceCooldown;

    public bool IsBeingHarvested { get; private set; } = false;
    private bool resourceHarvested = false;

    private HumanInventory humanInventory;

    public IEnumerator FarmResource(HumanInventory human)
    {
        humanInventory = human;

        if (resourceHarvested) yield break;

        IsBeingHarvested = true;

        yield return new WaitForSeconds(harvestTime);

        if (humanInventory != null)
        {
            humanInventory.AddRessource(resourceName, resourceAmount);

            if (humanInventory.isFullOfSomething)
            {
                VillageStorage villageStorage = humanInventory.GetComponent<HumanVillageInfos>().village.GetVillageStorage();
                if (villageStorage != null)
                {
                    humanInventory.TransferToVillageStorage(villageStorage);
                }
            }
        }

        resourceHarvested = true;
        IsBeingHarvested = false;

        if (!canRespawn)
        {
            WorldRessources.instance.UnregisterResource(this);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(ResourceRespawn());
        }
    }

    private IEnumerator ResourceRespawn()
    {
        yield return new WaitForSeconds(resourceCooldown);
        resourceHarvested = false;
    }

    private void OnEnable()
    {
        WorldRessources.instance.RegisterResource(this);
    }

    private void OnDisable()
    {
        if (WorldRessources.instance != null)
        {
            WorldRessources.instance.UnregisterResource(this);
        }
    }
}
