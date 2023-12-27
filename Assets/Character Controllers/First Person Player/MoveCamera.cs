//from tutorial: https://www.youtube.com/watch?v=f473C43s8nE&ab_channel=Dave%2FGameDevelopment


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform camPos;

    private void FixedUpdate()
    {
        transform.position = camPos.position;
    }
}
