using System.Collections;
using UnityEngine;

public class TreeParameters : MonoBehaviour
{
    public bool canGiveFood;
    public int foodGiven;
    public bool foodHarvested;
    public float foodCooldown;
    public float woodGiven;

    public void HarvestFood(HumanTimeManager human)
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
