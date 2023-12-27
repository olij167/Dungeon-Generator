using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatContainer
{
    public List<Stat> listOfStats;

    public Stat highestStat;

    [System.Serializable]
    public class Stat
    {
        public string statName = "New Stat";
        public float statValue;
    }

    [HideInInspector] public string newStatName = "PlayerStat";

    

}
