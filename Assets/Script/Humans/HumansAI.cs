using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class HumansAI : MonoBehaviour
{
    public HumansStats stats;
    public TimeManager timeManager;
    public (int year, int month) birthday;
    private bool isAdult;

    public float hunger;
    public float thirst;

    public NavMeshAgent agent;

    private enum HumanState { Idle, SearchingFood, SearchingWater }
    private HumanState currentState = HumanState.Idle;

    private TreeParameters currentTargetTree;
    private float interactionDistance = 1f;

    private bool isBusy = false;

    private void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        timeManager = GameObject.FindWithTag("Managers").GetComponent<TimeManager>();
        birthday = timeManager.GetDate();

        StartCoroutine(TimeBasedUpdates());
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
            default:
                break;
        }
    }

    IEnumerator TimeBasedUpdates()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeManager.monthDuration / timeManager.timeSpeed);

            hunger -= 5f;
            thirst -= 5f;

            (int currentYear, int currentMonth) = timeManager.GetDate();
            stats.currentAge = currentYear - birthday.year;

            if (stats.currentAge == 18 && currentMonth == birthday.month)
            {
                print(gameObject.name + " human is 18");
                isAdult = true;
            }

            if (!isBusy)
            {
                CheckNeeds();
            }
        }
    }

    void CheckNeeds()
    {
        if (hunger <= 40 && currentState != HumanState.SearchingFood)
        {
            currentState = HumanState.SearchingFood;
            isBusy = true;
        }
        else if (thirst <= 40 && currentState != HumanState.SearchingWater)
        {
            currentState = HumanState.SearchingWater;
            isBusy = true;
        }
        else
        {
            currentState = HumanState.Idle;
            isBusy = false;
        }
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
                isBusy = false;
            }
        }

        //Interact with tree
        if (currentTargetTree != null && Vector3.Distance(transform.position, currentTargetTree.transform.position) < interactionDistance)
        {
            currentTargetTree.HarvestFood(this);
            currentTargetTree = null;
            StartCoroutine(Interacting());
            isBusy = false;
            CheckNeeds();
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
            isBusy = false;
        }

        //Interact with water
        if(Vector3.Distance(transform.position, nearestWater) < interactionDistance)
        {
            thirst += 60;
            StartCoroutine(Interacting());
            isBusy = false;
            CheckNeeds();
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
        print("Idle");
    }
}
