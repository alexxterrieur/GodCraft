using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HumansAI : MonoBehaviour
{
    private enum HumanState { Idle, SearchingFood, SearchingWater, Eating, MovingToStorage }
    private HumanState currentState = HumanState.Idle;

    public NavMeshAgent agent;
    private HumanTimeManager humanTimeManager;
    private HumanInventory inventory;

    private ResourceParameters currentTargetResource;
    private float interactionDistance = 2f;
    private VillageStorage targetStorage;

    private void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        inventory = GetComponent<HumanInventory>();
        humanTimeManager = GetComponent<HumanTimeManager>();
    }


    private void Update()
    {
        switch (currentState)
        {
            case HumanState.MovingToStorage:
                CheckStorageProximity();
                break;
            case HumanState.SearchingFood:
                SearchFood();
                break;
            case HumanState.SearchingWater:
                SearchWater();
                break;
            case HumanState.Eating:
                EatFood();
                break;
            case HumanState.Idle:
                Idle();
                break;
        }
    }

    public void CheckNeeds(float hunger, float thirst)
    {
        if (currentState == HumanState.Idle)
        {
            if (hunger <= 40)
            {
                if (CheckForFoodInInventory())
                {
                    currentState = HumanState.Eating;
                }
                else
                {
                    currentState = HumanState.SearchingFood;
                }
            }
            else if (thirst <= 40)
            {
                currentState = HumanState.SearchingWater;
            }
        }

        if (currentState == HumanState.SearchingWater && thirst > 40)
        {
            currentState = HumanState.Idle;
        }

        if (currentState == HumanState.SearchingFood && hunger > 40)
        {
            currentState = HumanState.Idle;
        }
    }

    private bool CheckForFoodInInventory()
    {
        return inventory.currentFruits > 0 || inventory.currentMeats > 0; //Check for food in inventory
    }

    private void EatFood()
    {
        //Check if there's food in the inventory
        if (CheckForFoodInInventory())
        {
            if (inventory.currentMeats > 0)
            {
                inventory.RemoveResource("meat", 1);
                humanTimeManager.hunger = Mathf.Min(humanTimeManager.hunger + (humanTimeManager.maxHunger * 0.75f), humanTimeManager.maxHunger); //Set hunger to 75%
            }
            else if (inventory.currentFruits > 0)
            {
                inventory.RemoveResource("fruit", 1);
                humanTimeManager.hunger = humanTimeManager.maxHunger; //Set hunger to max value
            }
            currentState = HumanState.Idle;
        }
        else
        {
            currentState = HumanState.SearchingFood;
        }
    }

    public void UpdateAgentSpeed(float newSpeed)
    {
        agent.speed = newSpeed;
    }

    void SearchFood()
    {
        //Find fruit
        if (currentTargetResource == null || currentTargetResource.IsBeingHarvested)
        {
            currentTargetResource = WorldRessources.instance.FindNearestResource(transform.position, "fruit");

            if (currentTargetResource != null && !currentTargetResource.IsBeingHarvested)
            {
                agent.SetDestination(currentTargetResource.transform.position);

                //Subscribe to the resource's onResourceHarvested event
                currentTargetResource.onResourceHarvested.AddListener(OnResourceHarvested);
            }
            else
            {
                currentState = HumanState.Idle; //No resource found or it's being harvested
            }
        }

        //Interact with resource
        if (currentTargetResource != null && Vector3.Distance(transform.position, currentTargetResource.transform.position) < interactionDistance && !currentTargetResource.IsBeingHarvested)
        {
            StartCoroutine(currentTargetResource.FarmResource(inventory));
            currentTargetResource = null;
            StartCoroutine(Interacting());
        }
    }

    private void OnResourceHarvested()
    {
        //If inventory is full, move to the storage
        if (inventory.isFullOfSomething)
        {
            VillageStorage villageStorage = inventory.GetComponent<HumanVillageInfos>().village.GetVillageStorage();
            if (villageStorage != null)
            {
                MoveToVillageStorage(villageStorage);
                print("full, move to storage");
            }
        }

        //Unfollow the event when the interaction is over
        if (currentTargetResource != null)
        {
            currentTargetResource.onResourceHarvested.RemoveListener(OnResourceHarvested);
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
            print("No water found, returning to Idle state");
            currentState = HumanState.Idle;
            humanTimeManager.isBusy = false;
        }

        //Interact with water
        if (Vector3.Distance(transform.position, nearestWater) < interactionDistance)
        {
            humanTimeManager.thirst = humanTimeManager.maxThirst; //Set thirst to max value
            StartCoroutine(Interacting());
            humanTimeManager.isBusy = false;

            CheckNeeds(humanTimeManager.hunger, humanTimeManager.thirst);
        }
    }

    public void MoveToVillageStorage(VillageStorage villageStorage)
    {
        targetStorage = villageStorage;
        currentState = HumanState.MovingToStorage;
        agent.SetDestination(villageStorage.transform.position);
    }

    private void CheckStorageProximity()
    {
        if (targetStorage != null && Vector3.Distance(transform.position, targetStorage.transform.position) < interactionDistance)
        {
            inventory.TransferToVillageStorage(targetStorage);

            targetStorage = null;
            currentState = HumanState.Idle;
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
        print("Idle state");
    }
}
