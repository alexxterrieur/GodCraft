using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HumansAI : MonoBehaviour
{
    public HumansStats stats;
    public TimeManager timeManager;
    public (int year, int month) birthday;
    private bool isAdult;

    public float hunger = 100f;
    public float thirst = 100f;

    public NavMeshAgent agent;

    private enum HumanState { Idle, SearchingFood, SearchingWater }
    private HumanState currentState = HumanState.Idle;

    private TreeParameters currentTargetTree;
    private float interactionDistance = 3f;

    private void Start()
    {
        timeManager = GameObject.FindWithTag("Managers").GetComponent<TimeManager>();
        birthday = timeManager.GetDate();
        agent = gameObject.GetComponent<NavMeshAgent>();

        StartCoroutine(TimeBasedUpdates());
    }

    private void Update()
    {
        switch (currentState)
        {
            case HumanState.SearchingFood:
                SearchFood();
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

            if (stats.currentAge >= 18 && currentMonth == birthday.month)
            {
                print(gameObject.name + " human is 18");
                isAdult = true;
            }

            CheckNeeds();
        }
    }

    void CheckNeeds()
    {
        if (hunger < 15 && currentState != HumanState.SearchingFood)
        {
            currentState = HumanState.SearchingFood;
        }
        else if (thirst < 15 && currentState != HumanState.SearchingWater)
        {
            currentState = HumanState.SearchingWater;
        }
        else
        {
            currentState = HumanState.Idle;
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
                currentState = HumanState.Idle; //no tree found
            }
        }

        if (currentTargetTree != null && Vector3.Distance(transform.position, currentTargetTree.transform.position) < interactionDistance)
        {
            currentTargetTree.HarvestFood(this);
            currentState = HumanState.Idle;
        }
    }

    void Idle()
    {
        print("Idle");
    }
}
