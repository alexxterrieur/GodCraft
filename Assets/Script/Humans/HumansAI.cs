using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HumansAI : MonoBehaviour
{
    private enum HumanState { Idle, SearchingFood, SearchingWater }
    private HumanState currentState = HumanState.Idle;

    public NavMeshAgent agent;
    private HumanTimeManager humanTimeManager;
    private TreeParameters currentTargetTree;
    private float interactionDistance = 1f;

    private void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        humanTimeManager = GetComponent<HumanTimeManager>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case HumanState.SearchingFood:
                SearchFood();
                break;
            case HumanState.SearchingWater:
                SearchWater();
                break;
            case HumanState.Idle:
                Idle();
                break;
        }
    }

    public void CheckNeeds(float hunger, float thirst)
    {
        if (hunger <= 40 && currentState != HumanState.SearchingFood)
        {
            currentState = HumanState.SearchingFood;
        }
        else if (thirst <= 40 && currentState != HumanState.SearchingWater)
        {
            currentState = HumanState.SearchingWater;
        }
        else
        {
            currentState = HumanState.Idle;
        }
    }

    public void UpdateAgentSpeed(float newSpeed)
    {
        agent.speed = newSpeed;
    }

    void SearchFood()
    {
        if (currentTargetTree == null || currentTargetTree.foodHarvested)
        {
            currentTargetTree = WorldRessources.instance.FindNearestFoodTree(transform.position);
            if (currentTargetTree != null)
            {
                agent.SetDestination(currentTargetTree.transform.position);
            }
            else
            {
                currentState = HumanState.Idle;
            }
        }

        if (currentTargetTree != null && Vector3.Distance(transform.position, currentTargetTree.transform.position) < interactionDistance)
        {
            currentTargetTree.HarvestFood(humanTimeManager);
            currentTargetTree = null;
            StartCoroutine(Interacting());
        }
    }

    void SearchWater()
    {
        Vector3 nearestWater = WorldRessources.instance.FindNearestWater(transform.position);
        if (nearestWater != Vector3.zero)
        {
            agent.SetDestination(nearestWater);
        }
        else
        {
            currentState = HumanState.Idle;
            humanTimeManager.isBusy = false;
        }

        //Interact with water
        if (Vector3.Distance(transform.position, nearestWater) < interactionDistance)
        {
            humanTimeManager.thirst += 60;
            StartCoroutine(Interacting());
            humanTimeManager.isBusy = false;
            CheckNeeds(humanTimeManager.hunger, humanTimeManager.thirst);
        }
    }

    IEnumerator Interacting()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(1);
        agent.isStopped = false;
    }

    void Idle()
    {
        //rint("Idle");
    }
}
