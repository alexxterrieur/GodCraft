using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        if(gameObject.tag == "Human")
        {
            HumanVillageInfos villageInfo = gameObject.GetComponent<HumanVillageInfos>();

            if(villageInfo.belongsToVillage && villageInfo.village != null)
            {
                if (villageInfo.village.GetVillagerCount() == 1)
                {
                    Debug.Log("Village destroyed");
                    villageInfo.village.DestroyVillage();                    
                }

                if (villageInfo.village.GetVillagerCount() > 1 && villageInfo.isVillageChief)
                {
                    villageInfo.village.DesignateNewChief();
                }                

                villageInfo.village.RemoveVillager(villageInfo);                
            }

            print("Human dead");
            Destroy(gameObject);
        }
    }
}
