using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HouseLevelData", menuName = "ScriptableObjects/HouseLevelData")]
public class HouseLevelData : ScriptableObject
{
    public int maxResidentsNumber;
    public List<RessourceRequirement> ressourcesNeededToBuild;
    public Sprite sprite;
}
