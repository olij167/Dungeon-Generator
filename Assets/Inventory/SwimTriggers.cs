using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimTriggers : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private bool isUnderwaterTrigger;
    [SerializeField] private bool isSwimmingTrigger;
    //[SerializeField] private bool triggered;


    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 4)
        {
            if (isSwimmingTrigger)
            {
                player.isSwimming = true;
            }

            if (isUnderwaterTrigger)
            {
                player.isUnderwater = true;
            }
            //triggered = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 4)
        {
            if (isSwimmingTrigger)
            {
                player.isSwimming = false;
            }

            if (isUnderwaterTrigger)
            {
                player.isUnderwater = false;
            }
            //triggered = false;

        }
    }
}
