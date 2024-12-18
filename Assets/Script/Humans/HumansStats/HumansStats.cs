using UnityEngine;

[CreateAssetMenu(fileName = "HumansStats", menuName = "ScriptableObjects/HumanStats")]
public class HumansStats : ScriptableObject
{
    public int maxHealth = 100;
    public int currentAge = 18;
    public int lifeEspectancy = 110;
    public int strenght = 60;
    public float speed = 20;

}
