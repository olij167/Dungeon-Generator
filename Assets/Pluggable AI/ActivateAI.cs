using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAI : MonoBehaviour
{
    StateController controller;
    Vector3 startMovePoint = new Vector3();
    void Start()
    {
        controller = GetComponent<StateController>();
        startMovePoint = controller.GetRandomPointOnGraph();
        controller.detection.targetInLookSphere = false;

        controller.SetupAI(true, startMovePoint);

        Debug.Log("Ai Activated");
    }
}
