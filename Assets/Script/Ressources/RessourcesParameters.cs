using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class ResourceParameters : MonoBehaviour
{
    public string resourceName;
    public int resourceAmount = 10;
    public float harvestTime = 5f;

    public IEnumerator FarmResource()
    {
        // Simulate harvesting time
        yield return new WaitForSeconds(harvestTime);

        // Return harvested resources and destroy the resource
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