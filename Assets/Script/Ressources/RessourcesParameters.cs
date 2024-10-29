using UnityEngine;
using System.Collections;

public class ResourceParameters : MonoBehaviour
{
    public string resourceName;
    public int resourceAmount = 10;
    public float harvestTime = 5f;

    public bool IsBeingHarvested { get; private set; } = false;

    public IEnumerator FarmResource(HumansAI human)
    {
        IsBeingHarvested = true; 
        yield return new WaitForSeconds(harvestTime);

        //Give harvested resources and destroy the resource
        //humanInventory.Add(resourceName, resourceAmount);
        Debug.Log($"{resourceAmount} {resourceName} collected");
        WorldRessources.instance.UnregisterResource(this);
        Destroy(gameObject);
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
