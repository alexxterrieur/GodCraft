using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    private enum HumanState { Idle, SearchingFood, SearchingWater}
    private HumanState currentState = HumanState.Idle;


    private void Start()
    {
        timeManager = GameObject.FindWithTag("Managers").GetComponent<TimeManager>();
        birthday = timeManager.GetDate();

        StartCoroutine(TimeBasedUpdates());
    }

    private void Update()
    {
        
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
            Debug.Log(stats.currentAge);

            if (stats.currentAge >= 18 && currentMonth == birthday.month)
            {
                print(gameObject.name + "human is 18");
            }

            CheckNeeds();
        }        
    }

    void CheckNeeds()
    {
        if(hunger < 15)
        {
            currentState = HumanState.SearchingFood;
        }
        else if(thirst < 15)
        {
            currentState = HumanState.SearchingWater;
        }
        else
            currentState = HumanState.Idle;
    }

    void SearchFood()
    {
        print("searchingFood");
    }

    void SearchWater()
    {
        print("searchingWater");
    }

    void Idle()
    {
        print("Idle");
    }
}
