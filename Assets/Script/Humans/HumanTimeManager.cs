using System.Collections;
using UnityEngine;

public class HumanTimeManager : MonoBehaviour
{
    public HumansStats stats;
    public TimeManager timeManager;
    private (int year, int month) birthday;
    private bool isAdult;
    private HumansAI humanAI;

    public float hunger;
    public float thirst;
    public int survivalDamages;
    public bool isBusy = false;

    private LifeManager lifeManager;

    private void Awake()
    {
        timeManager = GameObject.FindWithTag("Managers").GetComponent<TimeManager>();
        timeManager.RegisterHuman(this);
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
            stats.currentAge = currentYear - birthday.year;

            if (stats.currentAge == 18 && currentMonth == birthday.month)
            {
                print(gameObject.name + " human is 18");
                isAdult = true;
            }

            if (stats.currentAge == stats.lifeEspectancy)
            {
                print(gameObject.name + " lifeEsperancy reach");
                lifeManager.TakeDamage(lifeManager.currentHealth);
            }

            if (!isBusy)
            {
                humanAI.CheckNeeds(hunger, thirst);
            }

            //Damages
            if (hunger <= 0 || thirst <= 0)
            {
                lifeManager.TakeDamage(survivalDamages);
            }
        }
    }

    public void SetHumanSpeed(float speedMultiplier)
    {
        humanAI.UpdateAgentSpeed(stats.speed * speedMultiplier);
    }

    //public void UpdateHunger(float amount) => hunger += amount;
    //public void UpdateThirst(float amount) => thirst += amount;
}
