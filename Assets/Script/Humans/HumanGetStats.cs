using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanGetStats : MonoBehaviour
{
    public HumansStats currentStats;

    private int maxHealth;
    private int lifeExpectancy;
    private int strength;
    private float speed;

    private LifeManager lifeManager;
    public bool spawnedByGod;
    public bool isAdult = false;

    private void Awake()
    {
        lifeManager = GetComponent<LifeManager>();
        UpdateStats();
    }

    public void UpdateStats()
    {
        if (currentStats == null) return;

        maxHealth = currentStats.maxHealth;
        lifeExpectancy = currentStats.lifeEspectancy;
        strength = currentStats.strenght;
        speed = currentStats.speed;

        lifeManager.maxHealth = maxHealth;
        lifeManager.currentHealth = maxHealth;
    }

    public int GetMaxHealth() => maxHealth;
    public int GetLifeExpectancy() => lifeExpectancy;
    public int GetStrength() => strength;
    public float GetSpeed() => speed;

    public void SetNewStats(HumansStats newStats)
    {
        if (!spawnedByGod && isAdult)
        {
            currentStats = newStats;
            UpdateStats();
        }
    }
}