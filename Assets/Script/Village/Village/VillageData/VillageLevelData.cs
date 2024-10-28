using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VillageLevelData", menuName = "ScriptableObjects/VillageLevelData")]
public class VillageLevelData : ScriptableObject
{
    //storage
    public int maxWoods;
    public int maxStones;
    public int maxOres;
    public int maxMeats;
    public int maxFruits;
    public int maxWater;

    //village
    public int maxHouses;
    public int maxHousesLevel;
    public HumansStats villagersStats;
    public List<RessourceRequirement> ressourcesNeededForNextLevel;

    public Sprite sprite;
}
