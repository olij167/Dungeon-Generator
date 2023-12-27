using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/AIStats")]
public class AIStats : ScriptableObject
{
    public GameObject aiPrefab;
    [Header("Movement Variables")]
    //public float maxSpeed = 25f;
    //public float acceleration = 5f;
    //public float turnSpeed = 25f;
    public float lookSphere = 10f;

    [Header("Behaviour Variables")]
    public float minStateTime = 30f;
    public float maxStateTime = 120f;

}
