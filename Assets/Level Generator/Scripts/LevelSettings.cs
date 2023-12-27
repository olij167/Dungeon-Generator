using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="LevelGenerator/ LevelSettings")]
[System.Serializable]
public class LevelSettings : ScriptableObject
{
    public string levelName;

    [SerializeField] private GameObject[] startPrefabs;
    [SerializeField] private GameObject[] tilePrefabs;
    [SerializeField] private GameObject[] exitPrefabs;
    [SerializeField] private GameObject[] blockedPrefabs;
    [SerializeField] private GameObject[] doorPrefabs;

    [Range(2, 100)] [SerializeField] private int mainLength = 10;
    [Range(0, 50)] [SerializeField] private int branchLength = 5;
    [Range(0, 50)] [SerializeField] private int numBranches = 10;
    [Range(0, 100)] [SerializeField] private int doorPercent = 25;
}
