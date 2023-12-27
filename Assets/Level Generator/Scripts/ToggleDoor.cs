using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleDoor : MonoBehaviour
{
    private Animator animator;
    private bool isInTrigger;

    public bool isLocked;
    public bool isOpen;

    private void Start()
    {
        animator = GetComponent<Animator>();

        
    }

    private void Update()
    {
        if (!isLocked)
        {
            if (isInTrigger && Input.GetKeyDown(KeyCode.E))
            {
                isOpen = animator.GetBool("isOpen");

                animator.SetBool("isOpen", !isOpen);
            }

            if (animator.GetBool("isOpen"))
            {
                transform.GetChild(0).GetComponent<Collider>().enabled = false;
            }
            else transform.GetChild(0).GetComponent<Collider>().enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInTrigger = false;
        }
    }

    public bool ToggleDoorLock()
    {
        return isLocked = !isLocked;
    }
}
