using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class ResourceParameters : MonoBehaviour
{
    [SerializeField] private string resourceName;
    [SerializeField] private int resourceAmount;
    [SerializeField] private float farmTime;

    private bool isHarvested = false;

    public bool CanHarvest() => !isHarvested;

    public int HarvestResource(NavMeshAgent agent)
    {
        if (!isHarvested)
        {
            StartCoroutine(FarmingTime(agent));
            isHarvested = true;
            return resourceAmount;
        }
        return 0;
    }

    private IEnumerator FarmingTime(NavMeshAgent agent)
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(farmTime);
        agent.isStopped = false;
    }

    public string GetResourceName() => resourceName;
}
