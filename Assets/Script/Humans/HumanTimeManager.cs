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

    // Reproduction variables
    [SerializeField] private GameObject babyHuman;
    [SerializeField] private int maxChildrenNumber;
    [SerializeField] private int menopauseAge;

    private float timeUntilNextBirth;
    private int childrenBorn = 0;
    private HumanVillageInfos humanVillageInfo;

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

        if (humanGetStats.spawnedByGod)
        {
            StartCoroutine(DelayedAdulthoodCheck());
        }

        StartCoroutine(TimeBasedUpdates());
    }

    private IEnumerator DelayedAdulthoodCheck()
    {
        yield return new WaitForSeconds(1f);
        CheckAndSetAdulthood();
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
                CheckAndSetAdulthood();
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

            // Vérification de la reproduction
            if (humanGetStats.isAdult && currentAge < menopauseAge)
            {
                timeUntilNextBirth -= 1;
                if (timeUntilNextBirth <= 0)
                {
                    TryReproduce(currentAge);
                }
            }
        }
    }

    private bool CheckAndSetAdulthood()
    {
        Debug.Log(gameObject.name + " is now an adult.");
        humanGetStats.isAdult = true;

        //Check if belongs to village and set stats (if not spawned by god)
        humanVillageInfo = GetComponent<HumanVillageInfos>();
        if (humanVillageInfo != null && humanVillageInfo.belongsToVillage && humanVillageInfo.village != null)
        {
            humanGetStats.SetNewStats(humanVillageInfo.village.GetVillagersStats());
        }
        else
        {
            humanGetStats.SetNewStats(defaultStats);
        }

        InitializeReproduction();

        return true;
    }

    private void InitializeReproduction()
    {
        childrenBorn = 0;
        timeUntilNextBirth = GetRandomBirthInterval();
    }

    private float GetRandomBirthInterval()
    {
        int startAge = 20;

        //Calculate birth interval
        float totalDuration = ((menopauseAge - startAge) * 12 * timeManager.monthDuration); //in months
        float minInterval = 12 * timeManager.monthDuration; //1 year minimum

        return Random.Range(minInterval, totalDuration / (maxChildrenNumber - childrenBorn));
    }

    private void TryReproduce(int currentAge)
    {
        if (humanVillageInfo != null && humanVillageInfo.belongsToVillage && humanVillageInfo.village != null)
        {
            var village = humanVillageInfo.village;

            //Find a partner in the village
            var partner = village.villagers.Find(h => h != humanVillageInfo && h.GetComponent<HumanTimeManager>().humanGetStats.isAdult);

            if (partner != null && babyHuman != null)
            {
                Debug.Log(gameObject.name + " and " + partner.name + " are reproducing.");

                GameObject newBaby = Instantiate(babyHuman, transform.position, Quaternion.identity);
                InitializeBaby(newBaby, village, gameObject, partner.gameObject);

                childrenBorn++;

                //Set a new time until nex birth
                if (childrenBorn < maxChildrenNumber)
                {
                    timeUntilNextBirth = GetRandomBirthInterval();
                }
                else
                {
                    timeUntilNextBirth = float.MaxValue; //Stop births
                }
            }
        }
    }

    private void InitializeBaby(GameObject baby, VillageManager village, GameObject father, GameObject mother)
    {
        var babyStats = baby.GetComponent<HumanGetStats>();
        var babyVillageInfo = baby.GetComponent<HumanVillageInfos>();
        var babyLifeManager = baby.GetComponent<LifeManager>();

        //Use default stats
        babyStats.isAdult = false;
        babyStats.spawnedByGod = false;
        babyStats.SetNewStats(defaultStats);

        //Add baby to village
        village.AddVillager(babyVillageInfo);

        var babyTimeManager = baby.GetComponent<HumanTimeManager>();
        babyTimeManager.hunger = 150f;
        babyTimeManager.thirst = 100f;

        //Random color
        Color skinColor = Color.Lerp(father.GetComponent<SpriteRenderer>().color, mother.GetComponent<SpriteRenderer>().color, 0.5f);
        baby.GetComponent<SpriteRenderer>().color = skinColor;
    }

    public void SetHumanSpeed(float speedMultiplier)
    {
        humanAI.UpdateAgentSpeed(humanGetStats.currentStats.speed * speedMultiplier);
    }
}