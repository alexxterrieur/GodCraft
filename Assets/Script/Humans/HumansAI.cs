using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HumansAI : MonoBehaviour
{
    private enum HumanState { Idle, SearchingFood, SearchingWater, Eating, MovingToStorage, Working }
    private HumanState currentState = HumanState.Idle;

    public enum Job { None, Lumberjack, Miner }
    public Job currentJob = Job.None;

    public NavMeshAgent agent;
    private HumanTimeManager humanTimeManager;
    private HumanInventory inventory;

    private ResourceParameters currentTargetResource;
    private float interactionDistance = 2f;
    private VillageStorage targetStorage;
    private bool isInteracting = false;

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
        if (isInteracting) return; // Prevent state changes while interacting

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
            case HumanState.Working:
                Work();
                break;
        }
    }

    public void UpdateAgentSpeed(float newSpeed)
    {
        agent.speed = newSpeed;
        agent.acceleration = newSpeed;
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
            else if (currentJob != Job.None)
            {
                currentState = HumanState.Working;
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
        return inventory.currentFruits > 0 || inventory.currentMeats > 0;
    }

    private void EatFood()
    {
        //Check if there's food in the inventory
        if (CheckForFoodInInventory())
        {
            isInteracting = true;
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
            StartCoroutine(Interacting()); //Interact coroutine to reset state
        }
        else
        {
            currentState = HumanState.SearchingFood;
        }
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
            HumanVillageInfos villageInfo = GetComponent<HumanVillageInfos>();

            if (!villageInfo.belongsToVillage)
                return;
            else
            {
                VillageStorage villageStorage = inventory.GetComponent<HumanVillageInfos>().village.GetVillageStorage();
                if (villageStorage != null)
                {
                    MoveToVillageStorage(villageStorage);
                    print("full, move to storage");
                }
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
            isInteracting = true;
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

    private void Work()
    {
        if (humanTimeManager.hunger <= 40) //Check if the human is hungry before continuing work
        {
            if (CheckForFoodInInventory())
            {
                currentState = HumanState.Eating; //Eat if food is available
            }
            else
            {
                currentState = HumanState.SearchingFood; //Search for food if none in inventory
            }
            return; //Do not continue working if hungry
        }

        if (currentJob == Job.Lumberjack)
        {
            SearchWood();
        }
        else if (currentJob == Job.Miner)
        {
            SearchStoneOrOre();
        }

        if (inventory.isFullOfSomething)
        {
            HumanVillageInfos villageInfo = GetComponent<HumanVillageInfos>();

            if (!villageInfo.belongsToVillage)
                return;
            else
            {
                VillageStorage villageStorage = inventory.GetComponent<HumanVillageInfos>().village.GetVillageStorage();
                if (villageStorage != null)
                {
                    MoveToVillageStorage(villageStorage);
                }
            }            
        }
    }

    private void SearchWood()
    {
        if (currentTargetResource == null || currentTargetResource.IsBeingHarvested)
        {
            currentTargetResource = WorldRessources.instance.FindNearestResource(transform.position, "wood");

            if (currentTargetResource != null && !currentTargetResource.IsBeingHarvested)
            {
                agent.SetDestination(currentTargetResource.transform.position);
                currentTargetResource.onResourceHarvested.AddListener(OnResourceHarvested);
            }
            else
            {
                currentState = HumanState.Idle; //No resource found
            }
        }

        if (currentTargetResource != null && Vector3.Distance(transform.position, currentTargetResource.transform.position) < interactionDistance && !currentTargetResource.IsBeingHarvested)
        {
            StartCoroutine(currentTargetResource.FarmResource(inventory));
            currentTargetResource = null;
            StartCoroutine(Interacting());
        }
    }

    private void SearchStoneOrOre()
    {
        if (currentTargetResource == null || currentTargetResource.IsBeingHarvested)
        {
            currentTargetResource = WorldRessources.instance.FindNearestResource(transform.position, "stone");

            //20% chance to target ore
            if (Random.value < 0.2f)
            {
                currentTargetResource = WorldRessources.instance.FindNearestResource(transform.position, "ore");
            }

            if (currentTargetResource != null && !currentTargetResource.IsBeingHarvested)
            {
                agent.SetDestination(currentTargetResource.transform.position);
                currentTargetResource.onResourceHarvested.AddListener(OnResourceHarvested);
            }
            else
            {
                currentState = HumanState.Idle; //No resource found or interacting
            }
        }

        if (currentTargetResource != null && Vector3.Distance(transform.position, currentTargetResource.transform.position) < interactionDistance && !currentTargetResource.IsBeingHarvested)
        {
            StartCoroutine(currentTargetResource.FarmResource(inventory));
            currentTargetResource = null;
            StartCoroutine(Interacting());
        }
    }

    IEnumerator Interacting()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(1);
        agent.isStopped = false;
        isInteracting = false; // Unlock state after interaction
    }

    void Idle()
    {
        print("Idle state");
    }
}
