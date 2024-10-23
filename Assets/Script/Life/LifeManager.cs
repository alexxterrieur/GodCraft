using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        if(gameObject.tag == "Human")
        {
            HumanVillageInfos villageInfo = gameObject.GetComponent<HumanVillageInfos>();

            if (villageInfo.belongsToVillage && villageInfo.village != null)
            {
                villageInfo.village.RemoveVillager(villageInfo);

                //if(villageInfo.village.GetVillagerCount() > 0)
                //{
                //    villageInfo.village.DesignateNewChief();
                //}
            }

            print("human dead");
            Destroy(gameObject);
        }
    }
}
