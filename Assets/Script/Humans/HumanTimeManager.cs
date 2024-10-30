using System.Collections;
using UnityEngine;

public class HumanTimeManager : MonoBehaviour
{
    private HumanGetStats humanGetStats;
    public TimeManager timeManager;
    private (int year, int month) birthday;
    private HumansAI humanAI;

    public float hunger;
    public float thirst;
    public int survivalDamages;
    public bool isBusy = false;

    private LifeManager lifeManager;
    [SerializeField] private HumansStats defaultStats;

    private void Awake()
    {
        timeManager = GameObject.FindWithTag("Managers").GetComponent<TimeManager>();
        timeManager.RegisterHuman(this);
        humanGetStats = GetComponent<HumanGetStats>();
    }

    private void Start()
    {
        humanAI = GetComponent<HumansAI>();
        
        birthday = timeManager.GetDate();
        lifeManager = GetComponent<LifeManager>();

        StartCoroutine(TimeBasedUpdates());
    }

    IEnumerator TimeBasedUpdates()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeManager.monthDuration / timeManager.timeSpeed);

            hunger -= 5f;
            thirst -= 5f;

            (int currentYear, int currentMonth) = timeManager.GetDate();
            int currentAge = currentYear - birthday.year;

            if (currentAge == 18 && currentMonth == birthday.month && !humanGetStats.isAdult)
            {
                Debug.Log(gameObject.name + " is now an adult.");
                humanGetStats.isAdult = true;

                //Check if belongs to village and set stats
                HumanVillageInfos humanVillageInfo = GetComponent<HumanVillageInfos>();
                if (humanVillageInfo != null && humanVillageInfo.belongsToVillage && humanVillageInfo.village != null)
                {
                    humanGetStats.SetNewStats(humanVillageInfo.village.GetVillagersStats());
                }
                else
                {
                    humanGetStats.SetNewStats(defaultStats);
                }
            }

            if (currentAge >= humanGetStats.GetLifeExpectancy())
            {
                Debug.Log(gameObject.name + " has reached life expectancy.");
                lifeManager.TakeDamage(lifeManager.currentHealth);
            }

            if (!isBusy)
            {
                humanAI.CheckNeeds(hunger, thirst);
            }

            if (hunger <= 0 || thirst <= 0)
            {
                lifeManager.TakeDamage(survivalDamages);
            }

            humanAI.CheckNeeds(hunger, thirst);
        }
    }

    public void SetHumanSpeed(float speedMultiplier)
    {
        humanAI.UpdateAgentSpeed(humanGetStats.currentStats.speed * speedMultiplier);
    }

    //public void UpdateHunger(float amount) => hunger += amount;
    //public void UpdateThirst(float amount) => thirst += amount;
}
