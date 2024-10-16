using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeParameters : MonoBehaviour
{
    public bool canGiveFood;
    public int foodGiven;
    private bool foodHarvested;
    public float foodCooldown;
    public float woodGiven;

    public void HarvestFood(HumansAI human)
    {
        if(canGiveFood && !foodHarvested)
        {
            foodHarvested = true;
            human.hunger += foodGiven;
            StartCoroutine(FoodRespawn());
        }        
    }

    IEnumerator FoodRespawn()
    {
        yield return new WaitForSeconds(foodCooldown);
        foodHarvested = false;
    }

    public void CutTree()
    {
        //human.Inventory.wood += woodGiven
        //Destroy(gameObject)
    }
}
