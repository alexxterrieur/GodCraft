using UnityEngine;
using System.Collections;
using UnityEngine.Events;

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

    //Event to notify that the resource has been harvested
    public UnityEvent onResourceHarvested;

    public IEnumerator FarmResource(HumanInventory human)
    {
        humanInventory = human;

        if (resourceHarvested) yield break;

        IsBeingHarvested = true;
        yield return new WaitForSeconds(harvestTime);

        if (humanInventory != null)
        {
            humanInventory.AddRessource(resourceName, resourceAmount);

            //Trigger the event to notify the resource was harvested
            onResourceHarvested?.Invoke();
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
